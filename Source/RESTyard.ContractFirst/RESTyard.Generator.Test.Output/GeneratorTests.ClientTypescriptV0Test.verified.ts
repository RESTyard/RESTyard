

export type Nullable<T> = T | undefined | null;

export function match<TIn, TOut>(nullable: Nullable<TIn>, onValue: (v: TIn) => TOut, onNothing: () => TOut): TOut {
    if (nullable === undefined || nullable === null) {
        return onValue(nullable as TIn);
    } else {
        return onNothing();
    }
}

export class TP1 {
    constructor(
        public readonly Property: Nullable<string>
    ) {  }
}

export class TP2 {
    constructor(
        public readonly Property: Nullable<string>
    ) {  }
}

export class TP3 {
    constructor(
        public readonly Property: Nullable<string>
    ) {  }
}

export class TP4 {
    constructor(
        public readonly Property: Nullable<string>
    ) {  }
}

export class TP11 extends TP1 {
    constructor(
        Property: Nullable<string>,
        public readonly Property2: Nullable<string>
    ) { super(Property); }
}

export class TP12 extends TP2 {
    constructor(
        Property: Nullable<string>,
        public readonly Property2: Nullable<string>
    ) { super(Property); }
}

export class TP13 extends TP3 {
    constructor(
        Property: Nullable<string>,
        public readonly Property2: Nullable<string>
    ) { super(Property); }
}

export class TP14 extends TP4 {
    constructor(
        Property: Nullable<string>,
        public readonly Property2: Nullable<string>
    ) { super(Property); }
}

export class WithProperties {
    constructor(
        public readonly Property: Nullable<string>,
        public readonly KeyProperty: Nullable<string>,
        public readonly OptionalProperty: Nullable<string>,
        public readonly KeyOptionalProperty: Nullable<string>
    ) {  }
}

export class DerivedWithProperties extends WithProperties {
    constructor(
        Property: Nullable<string>,
        KeyProperty: Nullable<string>,
        OptionalProperty: Nullable<string>,
        KeyOptionalProperty: Nullable<string>,
        public readonly DerivedProperty: Nullable<boolean>
    ) { super(Property, KeyProperty, OptionalProperty, KeyOptionalProperty); }
}

export class QueryHtoQuery {
    constructor(
        public readonly SomeInt: Nullable<int>
    ) {  }
}

export class BaseHco extends HypermediaObject {
    constructor(
        public readonly Id: Nullable<double>,
        public readonly Property: List<int>,
        public readonly item: ChildHco[],
        public readonly self: HypermediaLink<BaseHco>,
        public readonly dependency: HypermediaLink<ChildHco>,
        public readonly dependency2: Nullable<HypermediaLink<ChildHco>>,
        public readonly byQuery: HypermediaLink<QueryHco>,
        public readonly external: HypermediaLink<HypermediaObject>,
        public readonly Operation: Nullable<HypermediaAction>,
        public readonly WithParameter: Nullable<HypermediaAction<TP2>>,
        public readonly WithResult: Nullable<HypermediaFunction<ChildHco>>,
        public readonly WithParameterAndResult: Nullable<HypermediaFunction<ChildHco, External>>,
        public readonly Upload: Nullable<HypermediaAction>,
        public readonly UploadWithParameter: Nullable<HypermediaAction<TP12>>
    ) {
        super();
    }
}

export class ChildHco extends HypermediaObject {
    constructor(
        public readonly self: HypermediaLink<ChildHco>
    ) {
        super();
    }
}

export class DerivedHco extends ChildHco {
    constructor(
        public readonly self: HypermediaLink<DerivedHco>
    ) {
        super(self);
    }
}

export class NoSelfLinkHco extends HypermediaObject {
    constructor(
    ) {
        super();
    }
}

export class QueryHco extends HypermediaObject {
    constructor(
        public readonly NormalKey: Nullable<int>,
        public readonly QueryKey: Nullable<string>,
        public readonly NotAKey: Nullable<double>,
        public readonly self: HypermediaLink<QueryHco>
    ) {
        super();
    }
}

