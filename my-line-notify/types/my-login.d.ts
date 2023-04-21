import { LitElement } from 'lit';
/**
 * An example element.
 *
 * @slot - This element has a slot
 * @csspart button - The button
 */
export declare class MyLogin extends LitElement {
    render(): import("lit-html").TemplateResult<1>;
    private _LineLogin;
    static styles: import("lit").CSSResult;
}
declare global {
    interface HTMLElementTagNameMap {
        'my-login': MyLogin;
    }
}
