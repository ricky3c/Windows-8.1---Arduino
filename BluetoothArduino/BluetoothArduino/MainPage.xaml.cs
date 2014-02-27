using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using TCD.Controls;


namespace BluetoothArduino
{
    
    public sealed partial class MainPage : Page
    {
        //Hacemos instancia de la clase
        private ConexionBluetooth conexion = new ConexionBluetooth();
        // bandera que usaremos para saber el estado de la luz
        int banderaLuz = 0;
        public MainPage()
        {
            this.InitializeComponent();
            //declaramos nuestros eventos
            conexion.ExceptionOccured += delegate(object sender, Exception ex) { txtconsola.Text = ex.Message + "\n"; };
            conexion.MessageReceived += connectionManager_MessageReceived;
            conexion.StateChanged += connectionManager_StateChanged;
            conexion.State = BluetoothConnectionState.Disconnected;
        }
        protected override void OnNavigatedFrom(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            conexion.Disconnect();
            base.OnNavigatedFrom(e);
            progress.Visibility = Visibility.Collapsed;
        }
        //Evento para habilitar o desahbilitar los botones segun la conexion
        private void connectionManager_StateChanged(object sender, BluetoothConnectionState state)
        {
            progress.IsIndeterminate = (state == BluetoothConnectionState.Connecting);
            btncancelar.IsEnabled = (state == BluetoothConnectionState.Connecting);
            btndesconectar.IsEnabled = (state == BluetoothConnectionState.Connected);
        }
        //Evento donde se recibe el mensaje enviado desde el arduino y los mostrara en los textblock de estados
        //dependiendo del mensaje enviado
        private async void connectionManager_MessageReceived(object sender, string message)
        {
            if (message == "Luz Encendida")
            {
                txtled.Text = message;
            }
            else if (message == "Luz Apagada")
            {
                txtled.Text = message;
            }
            else if (message == "Servo a 180 Grados")
            {
                txtmotor.Text = message;
            }
            else if (message == "Servo a 0 Grados")
            {
                txtmotor.Text = message;
            }

        }
        //Boton de buscar muestra la lista de dispositivos encontrados solo es seleccionarlo para comenzar la conexion
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            await conexion.EnumerateDevicesAsync((sender as Button).GetElementRect());
            progress.Visibility = Visibility.Visible;
        }
        //Boton Cancelar cancela la conexion al bluetooth
        private void btncancelar_Click(object sender, RoutedEventArgs e)
        {
            conexion.AbortConnection();
            progress.Visibility = Visibility.Collapsed;
        }
        //Boton Desconectar 
        //desconecta la conexion con el bluetooth
        private void btndesconectar_Click(object sender, RoutedEventArgs e)
        {
            conexion.Disconnect();
            progress.Visibility = Visibility.Collapsed;
        }
        //Boton Led Envia Mensaje al arduino
        private async void btnled_Click(object sender, RoutedEventArgs e)
        {
            if (banderaLuz == 0)
            {
                var res = await conexion.SendMessageAsync("Encender_Luz");
                banderaLuz = 1;
            }
            else if (banderaLuz == 1)
            {
                var res = await conexion.SendMessageAsync("Apagar_Luz");
                banderaLuz = 0;
            }
        }
        //Envia Mensaje al arduino para mover el motor
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var res = await conexion.SendMessageAsync("Mover_Motor");
        }

    }
}
