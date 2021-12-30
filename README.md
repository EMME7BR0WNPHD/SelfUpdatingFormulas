# SelfUpdatingFormulas
A lightweight framework, that allows to create mutable variables and arrange them in formulas that are recalculated when any involved variable is changed.

Given classes help developing in cases when dealing with multiple interrelated variables.
As an example, let's consider the application, that helps plan the trip.
	We have the next set of variable:
Distance D
Fuel tank capacity T
Fuel consumption FC
Fuel economy FE
Amount of fuelt that is necessary for the whole trip V
Distance that could be covered in one tank DpT
Number of refills R

Amoung them there are variables, that does not depend upon any other - only user could set the value: T, D.
Another ones could be calculated:
DpT = T / FC
V = D * FC
R = V / T

There are also others, that could be both set up by the user or calculated: FC, FE.
FC = 1/FE
FE = 1/FC

Writing such a code that reacts on changes of variables and calculates all dependent ones could be difficult, and the resulting code itself could be complicated, error-prone (e.g. FE/FC calculations could cause looping). And even if the code is written, debugged and tested, any modification, or adding a new variable, could turn out a serious challenge.

But, using formulas, all these relations between variables could be written in compact and readable way:

			//  first, declaring all involved variables
            _distance = new MutableVariable<double>(4000d);
            _fuelTankCapacity = new MutableVariable<int>(40);
            _fuelConsumption = new MutableVariable<double>(5d);
            _fuelEconomy = new MutableVariable<double>(default);
            _maxDistancePerTank = new MutableVariable<double>(default);
            _volume = new MutableVariable<double>(default);
            _refillNumber = new MutableVariable< int>(default);

            //  then, set the relations using formulas:
            _fuelEconomy.SetCalculationFormula(() => 100 / _fuelConsumption * GallonToLiter / MileToKm);
            _fuelConsumption.SetCalculationFormula(() => 100 / _fuelEconomy * GallonToLiter / MileToKm);
            _maxDistancePerTank.SetCalculationFormula(() => _fuelTankCapacity / _fuelConsumption * 100);
            _volume.SetCalculationFormula(() => _distance * _fuelConsumption / 100);
            _refillNumber.SetCalculationFormula(() => (int) Math.Ceiling(_volume / _fuelTankCapacity));