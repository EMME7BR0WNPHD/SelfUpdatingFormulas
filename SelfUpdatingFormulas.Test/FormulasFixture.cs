using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace SelfUpdatingFormulas.Test
{
    [TestFixture]
    public class FormulasFixture
    {
        private const double MileToKm = 1.60934;
        private const double GallonToLiter = 3.78541;

        [Test]
        public void TestVariable()
        {
            var argument1 = new MutableVariable<int>(2);
            var argument2 = new MutableVariable<int>(3);
            Expression<Func<int>> sum = () => argument1 + argument2;

            var result1 = sum.Compile().Invoke();
            Assert.That(result1, Is.EqualTo(5));

            argument1.Value = 10;

            var result2 = sum.Compile().Invoke();
            Assert.That(result2, Is.EqualTo(13));
        }

        [Test]
        public void TestFormula()
        {
            var argument1 = new MutableVariable<int>(2);
            var argument2 = new MutableVariable<int>(3);
            var sum = new MutableVariable<int>(default, "sum");
            sum.SetCalculationFormula(() => argument1 + argument2);

            int result = 0;

            sum.Changed += delegate { result = sum.Value; };

            argument1.Value = 10;
            Assert.That(result, Is.EqualTo(13));

            argument2.Value = 5;
            Assert.That(result, Is.EqualTo(15));
        }

        [Test]
        public void TestFormulaDispose()
        {
            var argument1 = new MutableVariable<int>(2);
            var argument2 = new MutableVariable<int>(3);
            var sum = new MutableVariable<int>(default);

            var formula = sum.SetCalculationFormula(() => argument1 + argument2);

            int result = 0;

            sum.Changed += delegate { result = sum.Value; };

            argument1.Value = 10;
            Assert.That(result, Is.EqualTo(13));

            argument2.Value = 5;
            Assert.That(result, Is.EqualTo(15));

            formula.Dispose();

            argument2.Value = 100;
            Assert.That(result, Is.EqualTo(15));
        }

        [Test]
        public void TestFormulaWithFunctionInvocation()
        {
            var argument1 = new MutableVariable<int>(2);
            var argument2 = new MutableVariable<int>(3);
            var max = new MutableVariable<int>(default);
            max.SetCalculationFormula(() => Math.Max(argument2, argument1));

            int result = 0;

            max.Changed += delegate { result = max.Value; };

            argument1.Value = 5;
            Assert.That(result, Is.EqualTo(5));

            argument2.Value = 10;
            Assert.That(result, Is.EqualTo(10));
        }

        [Test]
        public void TestFormulaWithArray()
        {
            var argument1 = new MutableVariable<int>(2);
            var argument2 = new MutableVariable<int>(3);
            var array = new [] { argument1, argument2 };
            var max = new MutableVariable<int>(default);
            max.SetCalculationFormula(() => array.Max(x => x.Value));

            int result = 0;

            max.Changed += delegate { result = max.Value; };

            argument1.Value = 5;
            Assert.That(result, Is.EqualTo(5));

            argument2.Value = 10;
            Assert.That(result, Is.EqualTo(10));
        }

        [Test]
        public void TestFormulaWithObservableCollection()
        {
            var argument1 = new MutableVariable<int>(2);
            var argument2 = new MutableVariable<int>(3);
            var argument3 = new MutableVariable<int>(5);
            var observable = new ObservableCollection<MutableVariable<int>> {argument1, argument2};
            var max = new MutableVariable<int>(default);
            max.SetCalculationFormula(() => observable.Max(x => x.Value));

            int result = max.Value;

            max.Changed += delegate { result = max.Value; };

            Assert.That(result, Is.EqualTo(3));

            observable.Add(argument3);
            Assert.That(result, Is.EqualTo(5));
        }

        [Test]
        public void TestNestedFormulas()
        {
            var distanceKm = new MutableVariable<double>(1000, "distance [km]");
            var canisterVolume = new MutableVariable<double>(20, "canister volume [liters]");

            var litersPer100Km = new MutableVariable<int>(5, "litersPer100km");
            var distancePerCanister = new MutableVariable<double>(default, "distance by one canister [km]");

            var volume = new MutableVariable<double>(default, "volume [liters]");
            var numberOfCanisters = new MutableVariable<int>(default, "number of canisters");


            distancePerCanister.SetCalculationFormula(() => canisterVolume / litersPer100Km * 100);
            volume.SetCalculationFormula(() => distanceKm * litersPer100Km / 100);
            numberOfCanisters.SetCalculationFormula(() => (int)Math.Ceiling(volume / canisterVolume));

            int result = numberOfCanisters.Value;
            numberOfCanisters.Changed += (sender, args) => result = numberOfCanisters.Value;

            Assert.That(result, Is.EqualTo(3));

            Assert.That(distancePerCanister.Value, Is.EqualTo(400));

            litersPer100Km.Value = 10;

            Assert.That(result, Is.EqualTo(5));

            Assert.That(distancePerCanister.Value, Is.EqualTo(200));
        }

        [Test]
        public void TestCrossDependency()
        {

            //  declare variables:
            var distance = new MutableVariable<double>(4000d, "Distance");
            var fuelTankCapacity = new MutableVariable<double>(40, "FuelTankCapacity");
            var fuelConsumption = new MutableVariable<double>(5d, "FuelConsumptionLp100Km");
            var fuelEconomy = new MutableVariable<double>(default, "FuelEconomyMpG");
            var maxDistancePerTank = new MutableVariable<double>(default, "MaxDistancePerTank");
            var volume = new MutableVariable<double>(default, "Volume");
            var refillNumber = new MutableVariable<int>(default, "RefillNumber");

            //  set formulas:
            fuelEconomy.SetCalculationFormula(() => 100 / fuelConsumption * GallonToLiter / MileToKm);
            fuelConsumption.SetCalculationFormula(() => 100 / fuelEconomy * GallonToLiter / MileToKm);
            maxDistancePerTank.SetCalculationFormula(() => fuelTankCapacity / fuelConsumption * 100);
            volume.SetCalculationFormula(() => distance * fuelConsumption / 100);
            refillNumber.SetCalculationFormula(() => (int)Math.Ceiling(volume / fuelTankCapacity));

            double fuelConsumptionValue = default;
            double fuelEconomyValue = default;
            int refillNumberValue = default;
            fuelConsumption.Changed += (sender, args) => fuelConsumptionValue = fuelConsumption.Value;
            fuelEconomy.Changed += (sender, args) => fuelEconomyValue = fuelEconomy.Value;
            refillNumber.Changed += (sender, args) => refillNumberValue = refillNumber.Value;

            fuelEconomy.Value = 30;

            Assert.That(fuelConsumptionValue, Is.EqualTo(7.840501903471818d));
            Assert.That(refillNumberValue, Is.EqualTo(8));

            fuelConsumption.Value = 7;

            Assert.That(fuelEconomyValue, Is.EqualTo(33.602151014879219d));
            Assert.That(refillNumberValue, Is.EqualTo(7));
        }
    }
}
