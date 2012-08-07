enchant();

var game;
var man;
var map;
var sprites;

var Stone = enchant.Class.create(enchant.Sprite, {
    initialize: function(x, y){
        enchant.Sprite.call(this, 30, 30);
        this.image = game.assets['http://jsrun.it/assets/6/P/h/0/6Ph0B.jpg'];
        this.x = x; this.y = y;
        game.rootScene.addChild(this);
    }
});

var Block = enchant.Class.create(enchant.Sprite, {
    initialize: function(x, y){
        enchant.Sprite.call(this, 30, 30);
        this.image = game.assets['http://jsrun.it/assets/2/P/y/I/2PyID.jpg'];
        this.x = x; this.y = y;
        game.rootScene.addChild(this);
    }
});

var Man = enchant.Class.create(enchant.Sprite, {
    initialize: function(x, y){
        enchant.Sprite.call(this, 30, 30);
        this.image = game.assets['http://jsrun.it/assets/i/1/I/H/i1IHV.png'];
        this.xCoord = x; this.yCoord = y;
        this.x = this.xCoord*30;
        this.y = this.yCoord*30;
        this.bombs = 1;
        this.bombSize = 2;
        game.rootScene.addChild(this);
    },
    move: function(x, y){
      if (this.xCoord+x >= 0 && this.yCoord+y >= 0 && map[this.xCoord+x][this.yCoord+y] == 0) {
        this.xCoord += x;
        this.yCoord += y;
        this.x = this.xCoord*30;
        this.y = this.yCoord*30;
      }
    },
    putBomb: function() {
      if (this.bombs > 0) {
        this.bombs--;
        bomb = new Bomb(this.xCoord, this.yCoord);
      }
    }
});

var Bomb = enchant.Class.create(enchant.Sprite, {
  initialize: function(x, y) {
    enchant.Sprite.call(this, 30, 30);
    this.image = game.assets['http://jsrun.it/assets/f/8/j/z/f8jz5.jpg'];
    this.xCoord = x; this.yCoord = y;
    this.x = this.xCoord*30;
    this.y = this.yCoord*30;
    this.tick = 0;
    map[x][y] = 3;
    game.rootScene.addChild(this);
    this.addEventListener('enterframe', function(){
      this.tick++;
      if (this.tick > 2*game.fps) {
        this.explode();
      }
    });
  },
  explode: function() {
    man.bombs++;
    map[this.xCoord][this.yCoord] = 0;
    fire = new Fire(this.xCoord, this.yCoord);
    for (var i=1; i <= man.bombSize; i++) {
      fire = new Fire(this.xCoord-i, this.yCoord); 
      if (fire.spread == false) break;
    }
    for (var i=1; i <= man.bombSize; i++) {
      fire = new Fire(this.xCoord+i, this.yCoord); 
      if (fire.spread == false) break;
    }
    for (var i=1; i <= man.bombSize; i++) {
      fire = new Fire(this.xCoord, this.yCoord+i); 
      if (fire.spread == false) break;
    }
    for (var i=1; i <= man.bombSize; i++) {
      fire = new Fire(this.xCoord, this.yCoord-i); 
      if (fire.spread == false) break;
    }
    game.rootScene.removeChild(this);
  }
});

var Fire = enchant.Class.create(enchant.Sprite, {
  initialize: function(x, y) {
    this.spread = false;
    
    if (x < 0 || y < 0) return 0;
    if (map[x][y] == 1) return 0;
    
    enchant.Sprite.call(this, 30, 30);
    this.image = game.assets['http://jsrun.it/assets/u/n/L/T/unLTr.jpg'];
    this.xCoord = x; this.yCoord = y;
    this.x = this.xCoord*30;
    this.y = this.yCoord*30;
    this.tick = 0;
    if (map[x][y] != 0) {
      game.rootScene.removeChild(sprites[x][y]);
      sprites[x][y] = 0;
      this.spread = false;
    } else {this.spread = true;}
    map[x][y] = 4;
    
    game.rootScene.addChild(this);
    this.addEventListener('enterframe', function(){
      this.tick++;
      if (this.tick > 1*game.fps) {
        map[this.xCoord][this.yCoord] = 0;
        game.rootScene.removeChild(this);
      }
    });
  }
});

window.onload = function() {
    game = new Game(300, 300);
    map =	[[0,0,0,0,1,0,0,0,0,0],
          [0,1,0,1,1,0,1,1,1,0],
        	[0,1,0,0,1,0,0,0,0,0],
      		[0,1,0,1,0,1,0,1,0,0],
      		[0,0,0,0,0,0,0,1,0,1],
      		[1,1,0,1,1,1,0,1,0,1],
       		[1,0,0,0,1,0,0,0,0,1],
       		[0,0,1,0,0,0,0,1,0,0],
       		[0,1,1,1,0,1,1,1,1,0],
       		[0,0,0,0,0,0,0,0,0,0]];
       		
    sprites = [[0,0,0,0,1,0,0,0,0,0],
          [0,1,0,1,1,0,1,1,1,0],
        	[0,1,0,0,1,0,0,0,0,0],
      		[0,1,0,1,0,1,0,1,0,0],
      		[0,0,0,0,0,0,0,1,0,1],
      		[1,1,0,1,1,1,0,1,0,1],
       		[1,0,0,0,1,0,0,0,0,1],
       		[0,0,1,0,0,0,0,1,0,0],
       		[0,1,1,1,0,1,1,1,1,0],
       		[0,0,0,0,0,0,0,0,0,0]];
	
    game.fps = 24;
    game.preload('http://jsrun.it/assets/6/P/h/0/6Ph0B.jpg', 'http://jsrun.it/assets/f/8/j/z/f8jz5.jpg', 'http://jsrun.it/assets/i/1/I/H/i1IHV.png', 'http://jsrun.it/assets/2/4/1/x/241xk.jpg', 'http://jsrun.it/assets/u/n/L/T/unLTr.jpg', 'http://jsrun.it/assets/2/P/y/I/2PyID.jpg');
    // The images used in the game should be preloaded
    
  game.onload = function() {
    initMap();
    drawMap();
      man = new Man(0,0);
    }
    document.onkeydown = keyListener;
    
    game.start();
}

function initMap() {
    // Add blocks to the map
  for (var i=0; i<map.length; i++) {
    for (var n=0; n<map[0].length; n++) {
      if (map[i][n] == 0) 
        if (Math.random() > 0.7)
         	map[i][n] = 2;
    }
  }
}

function drawMap() {
  // Add stones and blocks to the map
for (var i=0; i<map.length; i++) {
  for (var n=0; n<map[0].length; n++) {
    if (map[i][n] == 1) {
     	sprites[i][n] = new Stone(i*30, n*30);	
    } else if (map[i][n] == 2){
        sprites[i][n] = new Block(i*30, n*30);
      }
  }
}

}

function keyListener(e) {
  e = e || window.event;
  switch(e.keyCode) {
    case 87: //W = up
      man.move(0, -1);
      break; 
    case 83: //S = down
      man.move(0, 1);
      break;
    case 65: //A = left
      man.move(-1, 0);
      break;
    case 68: //D = right
      man.move(1, 0);
      break;
    case 82: //R = bomb
      man.putBomb();
      break;
  }
}
