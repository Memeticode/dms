class AddNumber {
    constructor() {
      document.querySelector('#rndnum').innerText = randomNumberGenerator();
    }
  }
  
  function randomNumberGenerator() {
    return Math.random();
  }
  
  new AddNumber();
  