using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using System.Collections.ObjectModel;
using Microsoft.Maui.Dispatching;
using System.Linq;
using Plugin.BLE.Abstractions;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        private IAdapter adapter;
        private IDevice connectedDevice;
        public ObservableCollection<IDevice> Devices { get; private set; }

        public MainPage()
        {
            InitializeComponent();
            adapter = CrossBluetoothLE.Current.Adapter;
            Devices = new ObservableCollection<IDevice>();
            devicesList.ItemsSource = Devices;
        }

        private async Task ScanForDevicesAsync()
        {
            Devices.Clear();
            adapter.DeviceDiscovered += (s, a) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Devices.Add(a.Device);
                });
            };
            await adapter.StartScanningForDevicesAsync();
        }

        private async void OnDeviceSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is IDevice device)
            {
                try
                {
                    await adapter.ConnectToDeviceAsync(device);
                    connectedDevice = device;

                    if (device.State == DeviceState.Connected)
                    {
                        connectionStatusLabel.Text = $"Connected to {device.Name}";
                        bluetoothButton.Text = $"Search for Devices again and disconnect from {device.Name}";
                        devicesList.IsVisible = false;
                    }
                    else
                    {
                        connectionStatusLabel.Text = "Connection failed";
                        devicesList.IsVisible = true;
                    }
                }
                catch (DeviceConnectionException ex)
                {
                    connectionStatusLabel.Text = "Connection error";
                    devicesList.IsVisible = true;
                    await DisplayAlert("Error", $"Connection failed: {ex.Message}", "OK");
                }
            }
        }

        private async void OnBluetoothButtonClicked(object sender, EventArgs e)
        {
            if (connectedDevice != null)
            {
                var disconnectAlert = DisplayAlert("Disconnecting", $"Disconnecting from {connectedDevice.Name}...", "OK");
                await adapter.DisconnectDeviceAsync(connectedDevice);
                await disconnectAlert;

                connectedDevice = null;
                connectionStatusLabel.Text = "Disconnected";
                bluetoothButton.Text = "Scan for Devices";
            }

            await ScanForDevicesAsync();
            devicesList.IsVisible = true;
        }
    }
}
