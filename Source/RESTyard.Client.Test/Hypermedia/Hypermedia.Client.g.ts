
export type int = number;
export type double = number;
export type float = number;
export type IEnumerable<T> = T[];
export type List<T> = T[];
export class HypermediaObject { }
export class HypermediaLink<T> { }
export class HypermediaAction<T> { }
export class HypermediaFunction<T> { }
export class Country {
    constructor(
        public readonly Name: string
    ) { }
}
export class CustomerQuery { }
export class MarkAsFavoriteParameters { }

export type Nullable<T> = T | undefined | null;

export function match<TIn, TOut>(nullable: Nullable<TIn>, onValue: (v: TIn) => TOut, onNothing: () => TOut): TOut {
    if (nullable === undefined || nullable === null) {
        return onValue(nullable as TIn);
    } else {
        return onNothing();
    }
}

export class CreateCustomerParameters {
    constructor(
        public readonly Name: string
    ) {  }
}

export class BuyCarParameters {
    constructor(
        public readonly Brand: string,
        public readonly CarId: int,
        public readonly Price: Nullable<double>
    ) {  }
}

export class BuyLamborghiniParameters extends BuyCarParameters {
    constructor(
        public readonly Brand: string,
        public readonly CarId: int,
        public readonly Price: Nullable<double>,
        public readonly Color: string,
        public readonly OptionalProperty: Nullable<int>
    ) { super(Brand, CarId, Price); }
}

export class BuyLamborghinettaParameters extends BuyLamborghiniParameters {
    constructor(
        public readonly Brand: string,
        public readonly CarId: int,
        public readonly Price: Nullable<double>,
        public readonly Color: string,
        public readonly OptionalProperty: Nullable<int>,
        public readonly HorsePower: int
    ) { super(Brand, CarId, Price, Color, OptionalProperty); }
}

export class NewAddress {
    constructor(
        public readonly Address: string
    ) {  }
}

export class HypermediaEntrypointHco extends HypermediaObject {
    constructor(
        public readonly self: HypermediaLink<HypermediaEntrypointHco>,
        public readonly CustomersRoot: HypermediaLink<HypermediaCustomersRootHco>,
        public readonly CarsRoot: HypermediaLink<HypermediaCarsRootHco>
    ) {
        super();
    }
}

export class HypermediaCarsRootHco extends HypermediaObject {
    constructor(
        public readonly self: HypermediaLink<HypermediaCarsRootHco>,
        public readonly NiceCar: HypermediaLink<DerivedCarHco>,
        public readonly SuperCar: HypermediaLink<HypermediaCarHco>
    ) {
        super();
    }
}

export class HypermediaCarHco extends HypermediaObject {
    constructor(
        public readonly Id: Nullable<int>,
        public readonly Brand: Nullable<string>,
        public readonly PriceDevelopment: Nullable<IEnumerable<float>>,
        public readonly PopularCountries: Nullable<List<Country>>,
        public readonly MostPopularIn: Nullable<Country>,
        public readonly self: HypermediaLink<HypermediaCarHco>
    ) {
        super();
    }
}

export class DerivedCarHco extends HypermediaCarHco {
    constructor(
        public readonly Id: Nullable<int>,
        public readonly Brand: Nullable<string>,
        public readonly PriceDevelopment: Nullable<IEnumerable<float>>,
        public readonly PopularCountries: Nullable<List<Country>>,
        public readonly MostPopularIn: Nullable<Country>,
        public readonly DerivedProperty: Nullable<string>,
        public readonly item: HypermediaCustomerHco[],
        public readonly self: HypermediaLink<DerivedCarHco>,
        public readonly DerivedLink: Nullable<HypermediaLink<HypermediaCustomerHco>>,
        public readonly Derived: Nullable<HypermediaAction>
    ) {
        super(Id, Brand, PriceDevelopment, PopularCountries, MostPopularIn, self);
    }
}

export class NextLevelDerivedCarHco extends DerivedCarHco {
    constructor(
        public readonly Id: Nullable<int>,
        public readonly Brand: Nullable<string>,
        public readonly PriceDevelopment: Nullable<IEnumerable<float>>,
        public readonly PopularCountries: Nullable<List<Country>>,
        public readonly MostPopularIn: Nullable<Country>,
        public readonly DerivedProperty: Nullable<string>,
        public readonly item: HypermediaCustomerHco[],
        public readonly DerivedLink: Nullable<HypermediaLink<HypermediaCustomerHco>>,
        public readonly Derived: Nullable<HypermediaAction>,
        public readonly NextLevelDerivedProperty: Nullable<string>,
        public readonly self: HypermediaLink<NextLevelDerivedCarHco>
    ) {
        super(Id, Brand, PriceDevelopment, PopularCountries, MostPopularIn, DerivedProperty, item, self, DerivedLink, Derived);
    }
}

export class HypermediaCustomersRootHco extends HypermediaObject {
    constructor(
        public readonly self: HypermediaLink<HypermediaCustomersRootHco>,
        public readonly all: HypermediaLink<HypermediaCustomerQueryResultHco>,
        public readonly BestCustomer: HypermediaLink<HypermediaCustomerHco>,
        public readonly GreatSite: HypermediaLink<HypermediaObject>,
        public readonly CreateCustomer: Nullable<HypermediaFunction<HypermediaCustomerHco, CreateCustomerParameters>>,
        public readonly CreateQuery: Nullable<HypermediaAction<CustomerQuery>>
    ) {
        super();
    }
}

export class HypermediaCustomerHco extends HypermediaObject {
    constructor(
        public readonly Age: Nullable<int>,
        public readonly FullName: Nullable<string>,
        public readonly Address: Nullable<string>,
        public readonly IsFavorite: boolean,
        public readonly self: HypermediaLink<HypermediaCustomerHco>,
        public readonly CustomerMove: Nullable<HypermediaAction<NewAddress>>,
        public readonly CustomerRemove: Nullable<HypermediaAction>,
        public readonly MarkAsFavorite: Nullable<HypermediaAction<MarkAsFavoriteParameters>>,
        public readonly BuyCar: Nullable<HypermediaAction<BuyCarParameters>>
    ) {
        super();
    }
}

export class HypermediaCustomerQueryResultHco extends HypermediaObject {
    constructor(
        public readonly TotalEntities: Nullable<int>,
        public readonly CurrentEntitiesCount: Nullable<int>,
        public readonly item: HypermediaCustomerHco[],
        public readonly self: HypermediaLink<HypermediaCustomerQueryResultHco>
    ) {
        super();
    }
}

