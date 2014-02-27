//librerias
using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Popups;

namespace BluetoothArduino
{
    public class ConexionBluetooth
    {
        #region Eventos
        //Declaramos el delegado del cambio de estado del bluetooth 
        public delegate void AddOnStateChangedDelegate(object sender, BluetoothConnectionState state);
        public event AddOnStateChangedDelegate StateChanged;
        private void OnStateChangedEvent(object sender, BluetoothConnectionState state)
        {
            if (StateChanged != null)
                StateChanged(sender, state);
        }
        //Aqui declararemos el delegado de las exepciones 
        public delegate void AddOnExceptionOccuredDelegate(object sender, Exception ex);
        public event AddOnExceptionOccuredDelegate ExceptionOccured;
        private void OnExceptionOccuredEvent(object sender, Exception ex)
        {
            if (ExceptionOccured != null)
                ExceptionOccured(sender, ex);
        }
        //En este delegado recibira el mensaje enviado desde el arduino
        public delegate void AddOnMessageReceivedDelegate(object sender, string message);
        public event AddOnMessageReceivedDelegate MessageReceived;
        private void OnMessageReceivedEvent(object sender, string message)
        {
            if (MessageReceived != null)
                MessageReceived(sender, message);
        }
        #endregion

        #region Variables
        //Declaramos nuestras Variables
        private IAsyncOperation<RfcommDeviceService> connectService;
        private IAsyncAction connectAction;
        private RfcommDeviceService rfcommService;
        private StreamSocket socket;
        private DataReader reader;
        private DataWriter writer;

        private BluetoothConnectionState _State;
        public BluetoothConnectionState State
        {
            get { return _State; }
            set { _State = value; OnStateChangedEvent(this, value); }
        }
        #endregion

        #region Lifecycle
        //En esta metodo se muestran los dispositivos que sean del tipo SerialPort en un PopupMenu
        public async Task EnumerateDevicesAsync(Rect invokerRect)
        {
            this.State = BluetoothConnectionState.Enumerating;
            //seleccionamos todos los dispositivos disponibles
            var serviceInfoCollection = await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));
            //Creamos el PopupMenu en el que se mostraran los dispositivos
            PopupMenu menu = new PopupMenu();
            //Añadimos los dispositivos encontrados al PopupMenu
            foreach (var serviceInfo in serviceInfoCollection)
                menu.Commands.Add(new UICommand(serviceInfo.Name, new UICommandInvokedHandler(ConnectToServiceAsync), serviceInfo));
           //Seleccionamos el dispositvo con el que nos queremos comunicar
            var result = await menu.ShowForSelectionAsync(invokerRect);
            //Si no se pudo conectar al dispositivo se cambia el estado de la conexion a desconectado
            if (result == null)
                this.State = BluetoothConnectionState.Disconnected;
        }

        //Metodo que nos dara la conexion al dispositivo seleccionado
        private async void ConnectToServiceAsync(IUICommand command)
        {
            //Se obtiene el Id del dispositivo seleccionado
            DeviceInformation serviceInfo = (DeviceInformation)command.Id;
            //el estado de la conexion se pondra conectando
            this.State = BluetoothConnectionState.Connecting;
            try
            {
                //Inicializa el servicio del dispositivo RFCOMM de Bluetooth de destino
                connectService = RfcommDeviceService.FromIdAsync(serviceInfo.Id);
                rfcommService = await connectService;
                if (rfcommService != null)
                {
                    //Se inicializa el socket 
                    socket = new StreamSocket();
                    connectAction = socket.ConnectAsync(rfcommService.ConnectionHostName, rfcommService.ConnectionServiceName, SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);
                    //Puedes Cancelar la conexion 
                    await connectAction;
                    //Se inicializan las variables que envian y reciben los mensajes
                    writer = new DataWriter(socket.OutputStream);
                    reader = new DataReader(socket.InputStream);
                    Task listen = ListenForMessagesAsync();
                    //se cambia el estado de conexion del bluetooth a conectado
                    this.State = BluetoothConnectionState.Connected;
                }
                else
                    OnExceptionOccuredEvent(this, new Exception("No se pudo connectar al servicio.\n Verifca que 'bluetooth.rfcomm' capabilityes es declarado con la funcion de tipo 'name:serialPort' en Package.appxmanifest."));
            }
            catch (TaskCanceledException)
            {
                this.State = BluetoothConnectionState.Disconnected;
            }
            catch (Exception ex)
            {
                this.State = BluetoothConnectionState.Disconnected;
                OnExceptionOccuredEvent(this, ex);
            }
        }

            //Cancelar la conexion
        public void AbortConnection()
        {
            if (connectService != null && connectService.Status == AsyncStatus.Started)
                connectService.Cancel();
            if (connectAction != null && connectAction.Status == AsyncStatus.Started)
                connectAction.Cancel();
        }
           
        //Terminar la conexion con el dispositivo 
        public void Disconnect()
        {
            //dejamos variables en null
            if (reader != null)
                reader = null;
            if (writer != null)
            {
                writer.DetachStream();
                writer = null;
            }
            if (socket != null)
            {
                socket.Dispose();
                socket = null;
            }
            if (rfcommService != null)
                rfcommService = null;
            this.State = BluetoothConnectionState.Disconnected;
        }
        #endregion

        #region Enviar y Recivir
        //Metodo para enviar los mensajes al arduino
        public async Task<uint> SendMessageAsync(string message)
        {
            //Declaramos variable para ver tamaño del mensaje
            uint sentMessageSize = 0;
            if (writer != null)
            {
                //obtenemos el tamaño del string y lo convertimos en bytes y lo enviamos
                uint messageSize = writer.MeasureString(message);
                writer.WriteByte((byte)messageSize);
                sentMessageSize = writer.WriteString(message);
                await writer.StoreAsync();
            }
            return sentMessageSize;
        }
        private async Task ListenForMessagesAsync()
        {
            while (reader != null)
            {
                try
                {
                    uint sizeFieldCount = await reader.LoadAsync(1);
                    if (sizeFieldCount != 1)
                    {
                        // el socket se cierra antes de que se puedan leer los datos
                        return;
                    }

                    // Leer el mensaje 
                    uint messageLength = reader.ReadByte();
                    uint actualMessageLength = await reader.LoadAsync(messageLength);
                    if (messageLength != actualMessageLength)
                    {
                        // el socket se cierra antes de que se puedan leer los datos
                        return;
                    }
                    // mensaje leido y asignado a la variable message
                    string message = reader.ReadString(actualMessageLength);
                    // Se ejecuta el evento y retorna el mensaje 
                    OnMessageReceivedEvent(this, message);
                }
                catch (Exception ex)
                {
                    if (reader != null)
                        OnExceptionOccuredEvent(this, ex);
                }
            }
        }
        #endregion
    }
    //Tipos de estados de la conexion
    public enum BluetoothConnectionState
    {
        Disconnected,
        Connected,
        Enumerating,
        Connecting
    }
}
