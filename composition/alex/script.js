enchant();
var game;
var gameState;
var button;
var baseCard;
var subCards;
var cards;

var Card = enchant.Class.create(enchant.Sprite, {
  initialize: function(x, y, id){
    enchant.Sprite.call(this, 120, 160);
    this.x = x; 
    this.y = y;
    this.id = id;
    this.state = 0;
    this.image = game.assets['images/'+id+'.jpg'];
    game.rootScene.addChild(this);
    this.addEventListener("touchstart", function(){
      if (this.state == 1) { // base card was clicked
        this.state = 0;
        baseCard = null;
        id = this.id-1;
        this.tl.moveTo((id%5)*120, parseInt(id/5)*160+200, 5);
        game.rootScene.removeChild(button);
        gameState = 0;
        return true;
      }
      switch (gameState) {
        case 0: // selecting base card
          this.state = 1;
          this.tl.moveTo(180, 0, 5);
          gameState = 1;
          if (subCards.length > 0)
              game.rootScene.addChild(button);
          break;
        case 1: // selecting sub cards
          if (this.state == 0) { // becomes sub card
            this.state = 2;
            subCards.push(this.id);
            this.scale(0.8, 0.8);
            this.opacity = 0.5;
            if (subCards.length == 1)
              game.rootScene.addChild(button);
          } else if (this.state == 2) { // remove from sub cards
            this.state = 0;
            subCards.splice(subCards.indexOf(this.id), 1);
            this.scale(1.25, 1.25);
            this.opacity = 1;
            if (subCards.length == 0)
              game.rootScene.removeChild(button);
          }
      }
    });
  }, 
  changeId: function(id) {
    this.id = id;
    this.image = game.assets['images/'+id+'.jpg'];
  },
  animateAway: function() {
    this.tl.tween({opacity: 0.2, time:10}).moveTo(this.x, -180, 10).removeFromScene();
  },
  animateCenter: function() {
    this.tl.tween({x: 240, y: 300, scaleX: 2, scaleY: 2, time:20});
  },
  animateSub: function() {
    this.tl.tween({x: 0, y: 600, opacity: 1, time: 20}).moveTo(240, 320, 10);
  }
});

window.onload = function() {
    game = new Game(600, 900);
    game.fps = 24;
    for (var i=0; i<20; i++) {
      game.preload('images/'+(i+1)+'.jpg');
    }
    game.preload('images/bg.jpg', 'images/button.png');
    
  game.onload = function() {
    init();
  }
  game.start();
}

function init() {
  gameState = 0;
  button = new Sprite(160,60);
  button.image = game.assets['images/button.png'];
  button.x = 320;
  button.y = 50;

  button.addEventListener("touchstart", function(){
    doComposition();
  });

  cards = new Array();
  subCards = new Array();
  for (var i=0; i<21; i++) { // arrange the cards
    card = new Card((i%5)*120, parseInt(i/5)*160+200, i+1);
    cards.push(card);
  }
}

function doComposition() {
  game.rootScene.removeChild(button);
  for (var i=0; i<cards.length; i++) {
    card = cards[i];
    switch (card.state) {
      case 0: // not selected
        card.animateAway();
        break;
      case 1: // base card
        card.animateCenter();
        break;
      case 2: // sub card
        card.animateSub();
        break;
    }
  }
}