export type int = number;
export type double = number;
export type float = number;
export type IEnumerable<T> = T[];
export type List<T> = T[];
export type IList<T> = T[];
export class HypermediaObject { }
export class HypermediaLink<T> {
    constructor(public relations: string[], public url: string, public type: string) { }
}
export class HypermediaAction<T = undefined> { }
export class HypermediaFunction<TResult, TParameter = undefined> { }
export class Country {
    constructor(
        public readonly Name: string
    ) { }
}
export class CustomerQuery { }
export class MarkAsFavoriteParameters { }