using System;
using SelfUpdatingFormulas;

namespace ExampleApp.ViewModel
{
    public class TripCalculatorViewModel : ViewModelBase
    {
        private const double MileToKm = 1.60934;
        private const double GallonToLiter = 3.78541;

        private readonly MutableVariable<double> _distance;
        private readonly MutableVariable<double> _fuelConsumption;
        private readonly MutableVariable<double> _fuelEconomy;
        private readonly MutableVariable<int> _fuelTankCapacity;
        private readonly MutableVariable<double> _maxDistancePerTank;
        private readonly MutableVariable<double> _volume;
        private readonly MutableVariable<int> _refillNumber;

        public double Distance
        {
            get  => _distance.Value;
            set => _distance.Value = value;
        }

        public double FuelConsumptionLp100Km
        {
            get => _fuelConsumption.Value;
            set => _fuelConsumption.Value = value;
        }

        public double FuelEconomyMpG
        {
            get => _fuelEconomy.Value;
            set => _fuelEconomy.Value = value;
        }

        public int FuelTankCapacity
        {
            get => _fuelTankCapacity.Value;
            set => _fuelTankCapacity.Value = value;
        }

        public double MaxDistancePerTank => _maxDistancePerTank.Value;

        public double Volume => _volume.Value;

        public int RefillNumber => _refillNumber.Value;

        public TripCalculatorViewModel()
        {
            //  declare variables:
            _distance = Variable(4000d, nameof(Distance));
            _fuelTankCapacity = Variable(40, nameof(FuelTankCapacity));
            _fuelConsumption = Variable(5d, nameof(FuelConsumptionLp100Km));
            _fuelEconomy = Variable<double>(nameof(FuelEconomyMpG));
            _maxDistancePerTank = Variable<double>(nameof(MaxDistancePerTank));
            _volume = Variable<double>(nameof(Volume));
            _refillNumber = Variable<int>(nameof(RefillNumber));

            //  set formulas:
            _fuelEconomy.SetCalculationFormula(() => 100 / _fuelConsumption * GallonToLiter / MileToKm);
            _fuelConsumption.SetCalculationFormula(() => 100 / _fuelEconomy * GallonToLiter / MileToKm);
            _maxDistancePerTank.SetCalculationFormula(() => _fuelTankCapacity / _fuelConsumption * 100);
            _volume.SetCalculationFormula(() => _distance * _fuelConsumption / 100);
            _refillNumber.SetCalculationFormula(() => (int) Math.Ceiling(_volume / _fuelTankCapacity));
        }

        #region Helper methods
        private MutableVariable<T> Variable<T>(T initialValue, string propertyName)
        {
            var variable = new MutableVariable<T>(initialValue, propertyName);
            variable.Changed += (sender, args) => OnPropertyChanged(propertyName);
            return variable;
        }

        private MutableVariable<T> Variable<T>(string propertyName)
        {
            return Variable<T>(default, propertyName);
        }
        #endregion
    }
}