using BDL.Kettle.Model.Inputs;
using BDL.Kettle.Model.Outputs;
using BDL.Kettle.Model.Sensors;
using BDL.Kettle.Model.Workers;
using System;
using System.Threading.Tasks;

namespace BDL.Kettle.Controller
{
    public class KettleController
    {
        private readonly IPowerSwitch powerSwitch;
        private readonly IPowerLamp powerLamp;
        private readonly IHeatingElement heatingElement;
        private readonly IWaterSensor waterSensor;
        private readonly ITemperatureSensor temperatureSensor;

        public KettleController(IPowerSwitch powerSwitch,
            IPowerLamp powerLamp,
            IHeatingElement heatingElement,
            IWaterSensor waterSensor,
            ITemperatureSensor temperatureSensor)
        {
            this.powerSwitch = powerSwitch ?? throw new ArgumentNullException(nameof(powerSwitch));
            this.powerLamp = powerLamp ?? throw new ArgumentNullException(nameof(powerLamp));
            this.heatingElement = heatingElement ?? throw new ArgumentNullException(nameof(heatingElement));
            this.waterSensor = waterSensor ?? throw new ArgumentNullException(nameof(waterSensor));
            this.temperatureSensor = temperatureSensor ?? throw new ArgumentNullException(nameof(temperatureSensor));

            this.powerSwitch.SwitchedOn += PowerSwitch_SwitchedOn;
            this.powerSwitch.SwitchedOff += PowerSwitch_SwitchedOff;
            this.temperatureSensor.ValueChanged += TemperatureSensor_ValueChanged;
            this.waterSensor.ValueChanged += WaterSensor_ValueChanged;
        }

        private async void PowerSwitch_SwitchedOn(object sender, EventArgs e)
        {
            try
            {
                if (waterSensor.CurrentValue)
                {
                    await TurnOnKettle();
                }
                //activate the heating element and the lamp

                else if (!waterSensor.CurrentValue)
                {
                    await TurnOffKettle();
                    //if there is no water present
                }
            }
            catch (HeatingElementException)
            {
                await TurnOffKettle();
            }
            //heating element has a fault
        }
        private async void PowerSwitch_SwitchedOff(object sender, EventArgs e)
        {

            await TurnOffKettle();
        }
        // All components should be turned off when power switch is switched off.
        private async void WaterSensor_ValueChanged(object sender, Model.ValueChangedEventArgs<bool> e)
        {
            if (e.Value)
            {
                await TurnOffKettle();
            }
           
        }

        private async void TemperatureSensor_ValueChanged(object sender, Model.ValueChangedEventArgs<int> e)
        {
            if (e.Value >= 100)
            {
                await TurnOffKettle();
            } 
            }

        private async Task TurnOnKettle()
        {
            await powerSwitch.SwitchOnAsync();
            await heatingElement.SwitchOnAsync();
            await powerLamp.SwitchOnAsync();
        }

        private async Task TurnOffKettle()
        {
            await powerSwitch.SwitchOffAsync();
            await heatingElement.SwitchOffAsync();
            await powerLamp.SwitchOffAsync();
        }
       
    }
}
