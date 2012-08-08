enchant();
var game;
var man;
var map;
var sprites;

var Element = enchant.Class.create(enchant.Sprite, {
  initialize: function(x, y, type){      
    if (map[x][y] != 0 && type == 'fire') {
      map[x][y].explode();
      this.burned = true;
    }
    enchant.Sprite.call(this, 30, 30);
    this.xCoord = x; this.yCoord = y;
    this.x = this.xCoord*30; this.y = this.yCoord*30;
    if (this.itemtype == undefined)
      this.image = game.assets['images/'+type+'.jpg'];
    else this.image = game.assets['images/'+type+this.itemtype+'.jpg'];
    this.type = type;
    game.rootScene.addChild(this);
    if (map[x][y] == 0) map[x][y] = this;
    if (this.timer != 0) {
      this.tick = 0;
      this.addEventListener('enterframe', function(){
        this.tick++;
        if (this.tick > this.timer*game.fps) {
          this.explode();
        }
      });
    }
  }, 
  explode: function() {
    game.rootScene.removeChild(this);
    map[this.xCoord][this.yCoord] = 0;
    this.final();
  },
  final: function() {}

});

var Stone = enchant.Class.create(Element, {
    initialize: function(x, y){
        Element.call(this, x, y, 'stone');
    }
});

var Block = enchant.Class.create(Element, {
    initialize: function(x, y){
        Element.call(this, x, y, 'block');
    },
    final: function() {
      if (Math.random() > 0.2) {
        item = new Item(this.xCoord, this.yCoord);
      }
    }
});

var Item = enchant.Class.create(Element, {
    initialize: function(x, y){
      this.itemtype = Math.floor(Math.random() * 2)+1;
      Element.call(this, x, y, 'item');
    }
});

var Man = enchant.Class.create(Element, {
    initialize: function(x, y){
        Element.call(this, x, y, 'man');
        map[x][y] = 0;
        this.bombs = 1;
        this.bombSize = 2;
    },
    move: function(x, y){
      if (inBounds(this.xCoord+x, this.yCoord+y, false)) {
        this.xCoord += x; this.yCoord += y;
        this.x = this.xCoord*30; this.y = this.yCoord*30;
      }
      if (map[this.xCoord][this.yCoord] != 0) {
        if (map[this.xCoord][this.yCoord].type == 'item') {
          switch(map[this.xCoord][this.yCoord].itemtype) {
            case 1: // fire up
              this.bombSize++;
              break;
            case 2: // bomb up
              this.bombs++;
              break;
          }
          map[this.xCoord][this.yCoord].explode();
        } else if (map[this.xCoord][this.yCoord].type == 'fire') {
          alert('game over');
          game.stop();
        }
      }
    },
    putBomb: function() {
      if (this.bombs > 0) {
        this.bombs--;
        bomb = new Bomb(this.xCoord, this.yCoord);
      }
    }
});

var Bomb = enchant.Class.create(Element, {
  initialize: function(x, y) {
    this.timer = 2;
    Element.call(this, x, y, 'bomb');
  },
  final: function() {
    man.bombs++;
    fire = new Fire(this.xCoord, this.yCoord);
    var burned = [false, false, false, false];
    var dirs = [[-1,0],[1,0],[0,-1],[0,1]];
    for (var i=1; i <= man.bombSize; i++) {
      for (var dir=0; dir < 4; dir++) {
        if (!inBounds(this.xCoord+(i*dirs[dir][0]), this.yCoord+(i*dirs[dir][1]), true)) burned[dir] = true; 
        else if (!burned[dir]) {
          fire = new Fire(this.xCoord+(i*dirs[dir][0]), this.yCoord+(i*dirs[dir][1]));
          if (fire.burned) burned[dir]=true;
        }
      }
    }
  }
});

var Fire = enchant.Class.create(Element, {
  initialize: function(x, y) {
    if (x == man.xCoord && y == man.yCoord) {
      alert('game over');
      game.stop();
    }   
    this.timer = 1;
    Element.call(this, x, y, 'fire');    
  }, 
  explode: function(x,y) {
    if (map[this.xCoord][this.yCoord].type == 'fire') {
      map[this.xCoord][this.yCoord] = 0;
    }
    game.rootScene.removeChild(this);
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
    game.fps = 24;
    game.preload('images/block.jpg', 'images/bomb.jpg', 'images/man.jpg', 'images/fire.jpg', 'images/grass.jpg', 'images/item1.jpg', 'images/item2.jpg', 'images/stone.jpg');
    
  game.onload = function() {
      initMap();
      man = new Man(0,0);
    }
    document.onkeydown = keyListener;
    game.start();
}

function initMap() {
  // Add stones and blocks to the map
  for (var i=0; i<map.length; i++) {
    for (var n=0; n<map[0].length; n++) {
      if (map[i][n] == 1) {
        map[i][n] = 0;
       	stone = new Stone(i, n);	
      } else if (Math.random() > 0.7){
          block = new Block(i, n);
        }
    }
  }
}

function inBounds(x, y, burn) {
  if (x < 0 || y < 0 || x >= map.length || y >= map[0].length) return false;
  if (map[x][y] == 0) return true;
  if (map[x][y].type == 'stone') return false;
  if ((map[x][y].type == 'block' || map[x][y].type == 'bomb') && burn == false) return false;
  return true;
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
