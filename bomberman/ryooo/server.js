// Generated by CoffeeScript 1.3.3
(function() {
  var Block, Bomb, Char, Element, Enemy, Fire, Grass, Item, Movable, Stage, Stone, socketio,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  Stage = (function() {

    function Stage() {
      var x, y, _base, _i, _j;
      this.map = [];
      this.flow = [];
      this.char = {};
      this.diffA = [];
      this.diffD = [];
      this.charCanStay = [];
      this.frame = 0;
      for (y = _i = 0; _i <= 20; y = ++_i) {
        this.map[y] = [];
        this.flow[y] = [];
        for (x = _j = 0; _j <= 20; x = ++_j) {
          (_base = this.flow[y])[x] || (_base[x] = {});
          if ((x % 2 === 1) && (y % 2 === 1)) {
            this.map[y][x] = new Stone();
          } else {
            this.map[y][x] = new Grass();
            if (Math.random() <= 0.3) {
              this.flow[y][x] = new Block();
            } else {
              this.charCanStay.push({
                x: x,
                y: y
              });
            }
          }
        }
      }
    }

    Stage.prototype.bornChar = function(socket) {
      var id, pos;
      if (socket == null) {
        socket = null;
      }
      pos = this.charCanStay[parseInt(Math.random() * this.charCanStay.length)];
      if (socket === null) {
        id = parseInt(Math.random() * 10000);
        return this.char[id] = new Enemy(this, pos.x, pos.y, id);
      } else {
        return this.char[socket.id] = new Char(this, pos.x, pos.y, socket);
      }
    };

    Stage.prototype.deadChar = function(id) {
      this.char[id].socket.emit('char dead');
      return delete this.char[id];
    };

    Stage.prototype.isEmpty = function(x, y) {
      return this.getField(x, y).solid === false;
    };

    Stage.prototype.getField = function(x, y) {
      var _ref, _ref1;
      x = Math.max(Math.min(x, 20), 0);
      y = Math.max(Math.min(y, 20), 0);
      if ((_ref = this.map[y][x].type) === 's') {
        return this.map[y][x];
      }
      if ((_ref1 = this.flow[y][x].type) === 'b' || _ref1 === 'f' || _ref1 === 'o' || _ref1 === 'io' || _ref1 === 'if') {
        return this.flow[y][x];
      }
      return new Element();
    };

    Stage.prototype.getChar = function() {
      var id, obj, ret, _ref;
      ret = {};
      _ref = this.char;
      for (id in _ref) {
        obj = _ref[id];
        ret[id] = {
          id: id,
          x: obj.x,
          y: obj.y,
          type: obj.type
        };
      }
      return ret;
    };

    Stage.prototype.getFlowDiff = function() {
      var ret;
      ret = {
        add: this.diffA,
        "delete": this.diffD
      };
      this.diffA = [];
      this.diffD = [];
      return ret;
    };

    Stage.prototype.putBomb = function(owner, x, y) {
      return this.flow[y][x] = new Bomb(this, x, y, owner);
    };

    Stage.prototype.tick = function() {
      var char, id, x, y, _i, _j, _results;
      this.frame++;
      _results = [];
      for (y = _i = 0; _i <= 20; y = ++_i) {
        for (x = _j = 0; _j <= 20; x = ++_j) {
          if (typeof this.flow[y][x].limit === 'number') {
            if (this.flow[y][x].limit <= this.frame) {
              this.flow[y][x].limitReached();
            }
          }
        }
        _results.push((function() {
          var _ref, _results1;
          _ref = this.char;
          _results1 = [];
          for (id in _ref) {
            char = _ref[id];
            if (char instanceof Char) {
              if (this.getField(char.x, char.y).type === 'f') {
                this.deadChar(id);
              }
              if (this.enemyExist(char.x, char.y)) {
                _results1.push(this.deadChar(id));
              } else {
                _results1.push(void 0);
              }
            } else {
              _results1.push(void 0);
            }
          }
          return _results1;
        }).call(this));
      }
      return _results;
    };

    Stage.prototype.enemyExist = function(x, y) {
      var enemy, id, _ref;
      _ref = this.char;
      for (id in _ref) {
        enemy = _ref[id];
        if (enemy instanceof Enemy) {
          if (x === enemy.x && y === enemy.y) {
            return enemy;
          }
        }
      }
      return false;
    };

    Stage.prototype.fire = function(x, y) {
      var enemy, field, itemType, _ref;
      field = this.getField(x, y);
      if (field.type === 's') {
        return false;
      }
      itemType = null;
      if (field.type === 'b') {
        if (Math.random() <= 0.3) {
          itemType = Math.random() <= 0.5 ? 'if' : 'io';
        }
      }
      this.flow[y][x] = new Fire(this, x, y, itemType);
      if (field instanceof Bomb) {
        return field;
      }
      enemy = this.enemyExist(x, y);
      if (enemy !== false) {
        enemy.dead();
      }
      return !((_ref = field.type) === 'b' || _ref === 'io' || _ref === 'if');
    };

    return Stage;

  })();

  Element = (function() {

    function Element() {
      this.type = null;
      this.solid = false;
    }

    return Element;

  })();

  Stone = (function(_super) {

    __extends(Stone, _super);

    function Stone() {
      Stone.__super__.constructor.apply(this, arguments);
      this.type = 's';
      this.solid = true;
      this.gettable = false;
    }

    return Stone;

  })(Element);

  Grass = (function(_super) {

    __extends(Grass, _super);

    function Grass() {
      Grass.__super__.constructor.apply(this, arguments);
      this.type = 'g';
    }

    return Grass;

  })(Element);

  Block = (function(_super) {

    __extends(Block, _super);

    function Block() {
      Block.__super__.constructor.apply(this, arguments);
      this.type = 'b';
      this.solid = true;
    }

    return Block;

  })(Element);

  Fire = (function(_super) {

    __extends(Fire, _super);

    function Fire(stage, x, y, itemType) {
      Fire.__super__.constructor.apply(this, arguments);
      this.type = 'f';
      this.stage = stage;
      this.x = x;
      this.y = y;
      this.itemType = itemType;
      this.limit = stage.frame + 10;
      this.stage.diffD.push({
        x: x,
        y: y
      });
      this.stage.diffA.push({
        x: x,
        y: y,
        type: 'f'
      });
    }

    Fire.prototype.limitReached = function() {
      this.stage.flow[this.y][this.x] = {};
      this.stage.diffD.push({
        x: this.x,
        y: this.y
      });
      if (this.itemType) {
        return this.stage.flow[this.y][this.x] = new Item(this.stage, this.x, this.y, this.itemType);
      }
    };

    return Fire;

  })(Element);

  Bomb = (function(_super) {

    __extends(Bomb, _super);

    function Bomb(stage, x, y, owner) {
      Bomb.__super__.constructor.apply(this, arguments);
      this.stage = stage;
      this.x = x;
      this.y = y;
      this.owner = owner;
      this.limit = stage.frame + 30;
      this.type = 'o';
      this.solid = true;
      this.stage.diffA.push({
        x: x,
        y: y,
        type: 'o'
      });
    }

    Bomb.prototype.limitReached = function() {
      var bomb, chain, direction, i, ret, x, y, _i, _j, _k, _len, _len1, _ref, _ref1, _results;
      chain = [];
      this.owner.bomb--;
      this.stage.fire(this.x, this.y);
      _ref = ['u', 'd', 'l', 'r'];
      for (_i = 0, _len = _ref.length; _i < _len; _i++) {
        direction = _ref[_i];
        for (i = _j = 0, _ref1 = this.owner.fireLen; 0 <= _ref1 ? _j <= _ref1 : _j >= _ref1; i = 0 <= _ref1 ? ++_j : --_j) {
          x = this.x;
          y = this.y;
          if (direction === 'l') {
            x -= i;
          }
          if (direction === 'r') {
            x += i;
          }
          if (direction === 'u') {
            y -= i;
          }
          if (direction === 'd') {
            y += i;
          }
          x = Math.max(Math.min(x, 20), 0);
          y = Math.max(Math.min(y, 20), 0);
          ret = this.stage.fire(x, y);
          if (ret instanceof Bomb) {
            chain.push(ret);
          }
          if (ret === false) {
            break;
          }
        }
      }
      _results = [];
      for (_k = 0, _len1 = chain.length; _k < _len1; _k++) {
        bomb = chain[_k];
        _results.push(bomb.limitReached());
      }
      return _results;
    };

    return Bomb;

  })(Element);

  Item = (function(_super) {

    __extends(Item, _super);

    function Item(stage, x, y, type) {
      Item.__super__.constructor.apply(this, arguments);
      this.stage = stage;
      this.x = x;
      this.y = y;
      this.type = type;
      this.gettable = true;
      this.stage.diffA.push({
        x: x,
        y: y,
        type: type
      });
    }

    Item.prototype.get = function(char) {
      switch (this.type) {
        case 'if':
          char.fireLen++;
          break;
        case 'io':
          char.maxBomb++;
      }
      this.stage.diffD.push({
        x: this.x,
        y: this.y
      });
      return this.stage.flow[this.y][this.x] = {};
    };

    return Item;

  })(Element);

  Movable = (function() {

    function Movable(stage, x, y) {
      this.stage = stage;
      this.x = x;
      this.y = y;
    }

    Movable.prototype.move = function(x, y) {
      var newX, newY;
      newX = Math.max(Math.min(this.x + x, 20), 0);
      newY = Math.max(Math.min(this.y + y, 20), 0);
      if (this.stage.isEmpty(newX, newY)) {
        this.x = Math.max(Math.min(newX, 20), 0);
        return this.y = Math.max(Math.min(newY, 20), 0);
      }
    };

    return Movable;

  })();

  Char = (function(_super) {

    __extends(Char, _super);

    function Char(stage, x, y, socket) {
      Char.__super__.constructor.call(this, stage, x, y);
      this.bomb = 0;
      this.maxBomb = 2;
      this.fireLen = 2;
      this.socket = socket;
      this.type = 'c';
    }

    Char.prototype.putBomb = function() {
      if (this.bomb < this.maxBomb) {
        this.stage.putBomb(this, this.x, this.y);
        return this.bomb++;
      }
    };

    Char.prototype.move = function(x, y) {
      var field;
      Char.__super__.move.call(this, x, y);
      field = this.stage.getField(this.x, this.y);
      if (field.gettable) {
        return field.get(this);
      }
    };

    return Char;

  })(Movable);

  Enemy = (function(_super) {

    __extends(Enemy, _super);

    function Enemy(stage, x, y, id) {
      var _this = this;
      Enemy.__super__.constructor.call(this, stage, x, y);
      this.type = 'e';
      this.id = id;
      this.timer = setInterval(function() {
        var rand;
        rand = Math.random();
        if (0 < rand && rand <= 0.25) {
          _this.move(-1, 0);
        }
        if (0.25 < rand && rand <= 0.5) {
          _this.move(1, 0);
        }
        if (0.5 < rand && rand <= 0.75) {
          _this.move(0, -1);
        }
        if (0.75 < rand && rand <= 1) {
          _this.move(0, 1);
        }
        if (_this.stage.getField(_this.x, _this.y).type === 'f') {
          return _this.dead();
        }
      }, 1000);
    }

    Enemy.prototype.dead = function() {
      clearInterval(this.timer);
      return delete this.stage.char[this.id];
    };

    return Enemy;

  })(Movable);

  global.stage = new Stage();

  socketio = require('socket.io').listen(global.app);

  socketio.on('connection', function(socket) {
    var charId, i, _i;
    socket.on('message', function(val) {
      var char;
      char = global.stage.char[socket.id];
      if (typeof char === 'undefined') {
        return;
      }
      if (val === 'l') {
        char.move(-1, 0);
      }
      if (val === 'r') {
        char.move(1, 0);
      }
      if (val === 'u') {
        char.move(0, -1);
      }
      if (val === 'd') {
        char.move(0, 1);
      }
      if (val === 's') {
        return char.putBomb();
      }
    });
    socket.on('disconnect', function(v) {
      return global.stage.deadChar(socket.id);
    });
    if (typeof global.timer === 'undefined') {
      for (i = _i = 0; _i <= 3; i = ++_i) {
        global.stage.bornChar();
      }
      global.timer = setInterval(function() {
        var diff;
        global.stage.tick();
        diff = {
          flow: global.stage.getFlowDiff(),
          char: global.stage.getChar()
        };
        socket.emit('stage sync', diff);
        socket.broadcast.emit('stage sync', diff);
        if (Math.random() <= 0.005) {
          return global.stage.bornChar();
        }
      }, 100);
    }
    charId = global.stage.bornChar(socket);
    return socket.emit('stage init', {
      map: global.stage.map,
      flow: global.stage.flow,
      char: global.stage.getChar()
    });
  });

}).call(this);