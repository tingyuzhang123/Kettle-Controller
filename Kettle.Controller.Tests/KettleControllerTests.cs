using BDL.Kettle.Controller;
using BDL.Kettle.Model.Inputs;
using BDL.Kettle.Model.Outputs;
using BDL.Kettle.Model.Sensors;
using BDL.Kettle.Model.Workers;
using Moq;
using System;
using Xunit;

namespace Kettle.Controller.Tests
{
    public class KettleControllerTests
    {
        private readonly Mock<IPowerSwitch> powerSwitch;
        private readonly Mock<IPowerLamp> powerLamp;
        private readonly Mock<IHeatingElement> heatingElement;
        private readonly Mock<IWaterSensor> waterSensor;
        private readonly Mock<ITemperatureSensor> temperatureSensor;
        private readonly KettleController controller;

        public KettleControllerTests()
        {
            this.powerSwitch = new Mock<IPowerSwitch>();
            this.powerLamp = new Mock<IPowerLamp>();
            this.heatingElement = new Mock<IHeatingElement>();
            this.waterSensor = new Mock<IWaterSensor>();
            this.temperatureSensor = new Mock<ITemperatureSensor>();

            this.controller = new KettleController(this.powerSwitch.Object, 
            this.powerLamp.Object, 
            this.heatingElement.Object,
            this.waterSensor.Object,
            this.temperatureSensor.Object);
        }

        [Fact]
        //When the power switch is switched on, the kettle controller must activate the heating element and the lamp.
        public void KettleController_PowerSwitchOnActivatesHeatingElementAndPowerLamp()
        {
            this.powerSwitch.Setup(powerSwitch => powerSwitch.IsOn=false);
            this.temperatureSensor.Setup(tempratureSensor => tempratureSensor.CurrentValue=10); 
            this.waterSensor.Setup(waterSensor => waterSensor.CurrentValue=true);
            this.powerSwitch.Raise(powerSwitch => powerSwitch.SwitchedOn += null, EventArgs.Empty);
            this.heatingElement.Verify(h => h.SwitchOnAsync());
            this.powerLamp.Verify(p => p.SwitchOnAsync());
        }
        //When the power switch is switched off, all components must be deactivated.
        public void KettleController_PowerSwitchOffDectivatesHeatingElementAndPowerLamp()
        {
           
            this.powerSwitch.Raise(powerSwitch => powerSwitch.SwitchedOff += null, EventArgs.Empty);

            this.heatingElement.Verify(h => h.SwitchOffAsync());
            this.powerLamp.Verify(p => p.SwitchOffAsync());
        }
        //The power switch is a toggle switch and must be switched off by the controller in any case where the controller decides to turn off the kettle.
        public void KettleController_ToggleSwitch()
        {
            this.powerSwitch.Setup(powerSwitch => powerSwitch.IsOn = true);
            this.temperatureSensor.Setup(tempratureSensor => tempratureSensor.CurrentValue = 30);
            this.waterSensor.Setup(waterSensor => waterSensor.CurrentValue = true);
            this.powerSwitch.Raise(powerSwitch => powerSwitch.SwitchedOff += null, EventArgs.Empty);
            this.heatingElement.Verify(h => h.SwitchOffAsync());
            this.powerLamp.Verify(p => p.SwitchOffAsync());
        }
        //The kettle controller should not attempt to heat if there is no water present
        public void KettleController_NoWater()
        {
            this.waterSensor.Setup(waterSensor => waterSensor.CurrentValue=false);
            this.powerSwitch.Raise(powerSwitch => powerSwitch.SwitchedOff += null, EventArgs.Empty);
            this.heatingElement.Verify(h => h.SwitchOffAsync());
            this.powerLamp.Verify(p => p.SwitchOffAsync());
        }
        //When the heating element has a fault, it will raise HeatingElementException when switching on, and the controller must ensure that all the components are off.
        public void KettleController_HeatingElementHasAFault()
        {
            try
            {
                this.powerSwitch.Setup(powerSwitch => powerSwitch.IsOn = false);
                this.waterSensor.Setup(waterSensor => waterSensor.CurrentValue = true);
                this.temperatureSensor.Setup(tempratureSensor => tempratureSensor.CurrentValue = 30);
                this.powerSwitch.Verify(powerSwitch => powerSwitch.SwitchedOnAsync());
                this.heatingElement.Verify(h => h.SwitchOnAsync());
                this.powerLamp.Verify(p => p.SwitchOnAsync());
            }
            catch (HeatingElementException)
            {
                Assert.Fail("There is a fault in the heating element");
                this.powerSwitch.Raise(powerSwitch => powerSwitch.SwitchedOff += null, EventArgs.Empty);
                this.heatingElement.Verify(h => h.SwitchOffAsync());
                this.powerLamp.Verify(p => p.SwitchOffAsync());
            }
        }
        //When the water temperature reaches 100 degrees Celsius, the power must be switched off.
        public void KettleController_TemperatureReaches100DegreesCelsius()
        {

            this.powerSwitch.Setup(powerSwitch => powerSwitch.IsOn = true);
            this.waterSensor.Setup(waterSensor => waterSensor.CurrentValue = true);
            this.temperatureSensor.Setup(tempratureSensor => tempratureSensor.CurrentValue = 99);
            this.temperatureSensor.Setup(tempratureSensor => tempratureSensor.ValueChanged += null, Model.ValueChangedEventArgs<int> = 100);
            this.powerSwitch.Raise(powerSwitch => powerSwitch.SwitchedOff += null, EventArgs.Empty);
            this.heatingElement.Verify(h => h.SwitchOffAsync());
            this.powerLamp.Verify(p => p.SwitchOffAsync());
        }
        //should switch off if the water is removed during heating.
        public void KettleController_WaterIsRemoved()
        {

            this.powerSwitch.Setup(powerSwitch => powerSwitch.IsOn = true);
            this.waterSensor.Setup(waterSensor => waterSensor.CurrentValue = true);
            this.temperatureSensor.Setup(tempratureSensor => tempratureSensor.CurrentValue = 60);
            this.waterSensor.Raise(waterSensor => waterSensor.ValueChanged += null, Model.ValueChangedEventArgs<bool>=true);
            this.powerSwitch.Raise(powerSwitch => powerSwitch.SwitchedOff += null, EventArgs.Empty);
            this.heatingElement.Verify(h => h.SwitchOffAsync());
            this.powerLamp.Verify(p => p.SwitchOffAsync());
        }
    }
    }
}
