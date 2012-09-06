enchant();
var game;
var map;
var shop;
var selectedCell;
var money;
var hp;
var tick;
var numKilled;
var spawnTime;

var Cell = enchant.Class.create(enchant.Sprite, {
  initialize: function(x, y){
    enchant.Sprite.call(this, 30, 30);
    this.x = x*30; 
    this.y = y*60;
    this.type = 0;
    this.image = game.assets['images/cell.jpg'];
    game.rootScene.addChild(this);
    this.addEventListener("touchstart", function(){
      selectedCell = this;
      this.image = game.assets['images/stone.jpg'];
      game.pushScene(shop);
    });
  }, 
  addTower: function(type) {
    switch(type) {
      case 1: // cheap tower
        if (money >= 10) {
          this.image = game.assets['images/tower1.jpg'];
          map[this.y/60][this.x/30] = 2;
          money -= 10;
          document.getElementById("money").innerHTML = money;
          game.popScene(shop);
        }
        break;
      case 2: // cheap tower
        if (money >= 100) {
          this.image = game.assets['images/tower2.jpg'];
          map[this.y/60][this.x/30] = 4;
          game.popScene(shop);
          money -= 100;
          document.getElementById("money").innerHTML = money;
        }
        break;
    }
  }
});

var Enemy = enchant.Class.create(enchant.Sprite, {
  initialize: function(x, y){
    enchant.Sprite.call(this, 30, 30);
    this.x = 300; 
    this.y = 30;
    this.speed = (numKilled/15)+1;
    this.hp = 100+(numKilled*4);
    this.image = game.assets['images/enemy.jpg'];
    game.rootScene.addChild(this);
    this.addEventListener('enterframe', function(){
      this.x -= this.speed;
      this.hp -= map[0][parseInt(this.x/30)]+map[1][parseInt(this.x/30)];
      this.opacity = this.hp/100;
      if (this.hp <= 0) {
        game.rootScene.removeChild(this);
        money += 10;
        numKilled++;
        if (spawnTime > 10) spawnTime -= .5;
        document.getElementById("money").innerHTML = money;
        document.getElementById("level").innerHTML = parseInt(numKilled/5)+1;
      }
      if (this.x <= 0) {
        game.rootScene.removeChild(this);
        hp -= 10;
        document.getElementById("hp").innerHTML = hp;
      }
    });
  }
});

window.onload = function() {
    game = new Game(300, 300);
    game.fps = 24;
    game.preload('images/enemy.jpg', 'images/cell.jpg', 'images/stone.jpg', 'images/tower1.jpg', 'images/tower2.jpg', 'images/tower1Shop.png', 'images/tower2Shop.png');
    
  game.onload = function() {
    init();
    tick = 0;
    game.addEventListener('enterframe', function(){
      tick++;
      if (tick > spawnTime) {
        tick = 0;
        enemy = new Enemy();
      }
    });
  }
  game.start();
}

function init() {
  // Add cells to the map
  map = new Array(
    new Array(0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
    new Array(0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
  );
  for (var i=0; i<10; i++) {
    cell = new Cell(i, 0);
    cell = new Cell(i, 1);
  }
  // Add shop
  shop = new Scene();
  tower1 = new Sprite(80, 80);
  tower1.image = game.assets['images/tower1Shop.png'];
  tower1.x = 0; tower1.y = 0;
  tower1.addEventListener("touchstart", function(){
      selectedCell.addTower(1);
    });
  shop.addChild(tower1);
  tower2 = new Sprite(80, 80);
  tower2.image = game.assets['images/tower2Shop.png'];
  tower2.x = 90; tower2.y = 0;
  tower2.addEventListener("touchstart", function(){
      selectedCell.addTower(2);
    });
  shop.addChild(tower2);
  closeLabel = new Label('閉じる');
  closeLabel.x = 160;
  closeLabel.y = 0;
  closeLabel.addEventListener("touchstart", function(){
    selectedCell.image = game.assets['images/cell.jpg'];
      game.popScene(shop);
    });
  shop.addChild(closeLabel);
  shop.x = 100;
  shop.y = 100;
  // Set params
  money = 20;
  hp = 100;
  numKilled = 0;
  spawnTime = 50;
}