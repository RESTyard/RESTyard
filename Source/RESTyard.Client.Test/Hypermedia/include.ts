
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