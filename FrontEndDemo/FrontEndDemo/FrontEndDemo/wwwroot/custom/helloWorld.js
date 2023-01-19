export default class HelloWorld extends HTMLElement {
    constructor() {
        // needed in every constructor which extends another class
        super();

        // do magic here
        this.innerText = 'Hello World';
    }
}