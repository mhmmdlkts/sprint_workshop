var mapWidth = 10;
var mapHeight = 20;
var map = new Array();
var stage = document.getElementById('stage');
var timer;
var allCoords = new Array(
  //square
  Array(
    Array(Array(0, -1), Array(1, -1), Array(1, 0), Array(0, 0)),
    Array(Array(0, -1), Array(1, -1), Array(1, 0), Array(0, 0)),
    Array(Array(0, -1), Array(1, -1), Array(1, 0), Array(0, 0)),
    Array(Array(0, -1), Array(1, -1), Array(1, 0), Array(0, 0))
  ),
  //Left S
  Array(
    Array(Array(0,0), Array(1,0), Array(1,-1), Array(2,-1)),
    Array(Array(0,-2),Array(0,-1),Array(1,-1),Array(1,0)),
    Array(Array(0,0), Array(1,0), Array(1,-1), Array(2,-1)),
    Array(Array(0,-2),Array(0,-1),Array(1,-1),Array(1,0))
  ),
  //Right S
  Array(
    Array(Array(0,-1), Array(1,-1), Array(1,0), Array(2,0)),
    Array(Array(0,0),Array(0,-1),Array(1,-1),Array(1,-2)),
    Array(Array(0,-1), Array(1,-1), Array(1,0), Array(2,0)),
    Array(Array(0,0),Array(0,-1),Array(1,-1),Array(1,-2))
  ),
  //Right L
  Array(
    Array(Array(0,0), Array(1,0), Array(2,0), Array(2,-1)),
    Array(Array(0,0),Array(0,-1),Array(0,-2),Array(1,0)),
    Array(Array(0,-2),Array(1,-2),Array(1,-1),Array(1,0)),
    Array(Array(0,0),Array(0,-1),Array(1,-1),Array(2,-1))
  ),
  //Left L
  Array(
    Array(Array(0,0), Array(1,0), Array(2,0), Array(2,1)),
    Array(Array(0,0),Array(1,0),Array(1,-1),Array(1,-2)),
    Array(Array(0,0), Array(1,0), Array(2,0), Array(0,-1)),
    Array(Array(0,0),Array(0,-1),Array(0,-2),Array(1,-2))
  ),
  //Line
  Array(
    Array(Array(0,0), Array(1,0), Array(2,0), Array(3,0)),
    Array(Array(0,0),Array(0,-1),Array(0,-2),Array(0,-3)),
    Array(Array(0,0), Array(1,0), Array(2,0), Array(3,0)),
    Array(Array(0,0),Array(0,-1),Array(0,-2),Array(0,-3))
  )
);

var block = {
  x: 4,
  y: 1,
  shape: 0,
  position: 0,
  element: document.getElementById("block"),
  coords: new Array(),
  
  create: function() {
    this.x = 4;
    this.y = 1;
    this.position = 0;
    this.shape = Math.floor(Math.random()*allCoords.length);
    this.coords = allCoords[this.shape][this.position];
    this.redraw();
    timer = setTimeout(block.drop, 500);
  },
  
  drop: function() {
    block.move(0, 1, true);
    clearInterval(timer);
    timer = setTimeout(block.drop, 500);
  },
  
  rotate: function() {
    this.position = (this.position+1) % 4;
    this.coords = allCoords[this.shape][this.position];
    this.redraw();
  },
  
  move: function(x, y, drop) {
    if (this.check(this.x+x, this.y+y)) {
      this.x += x;
      this.y += y;
      this.draw();
    } else if (drop) {
      fillMap();
      checkRows();
      block.create();
    }
  },
  
  check: function(x, y) {
    for (i=0; i<this.coords.length; i++) {
      if (x+this.coords[i][0] < 0 || x+this.coords[i][0] >= mapWidth)
        return false;
      if (y+this.coords[i][1] < 0 || y+this.coords[i][1] >= mapHeight)
        return false;
      if (map[y+this.coords[i][1]][x+this.coords[i][0]] != 0)
        return false;
    }
    return true;
  },
  
  draw: function() {
    this.element.style.left = this.x * 10;
    this.element.style.top = this.y * 10;
  },
  
  redraw: function() {
    this.element.innerHTML = "";
    for(i=0; i<this.coords.length; i++) {
      var newdiv = document.createElement('div');
      newdiv.setAttribute('class', 'square piece');
      newdiv.style.left = this.coords[i][0]*10;
      newdiv.style.top = this.coords[i][1]*10;
      this.element.appendChild(newdiv);
      this.draw();
    }
  }
}

function init() {
  for (i=0; i<mapHeight; i++) {
    map.unshift(new Array(0,0,0,0,0,0,0,0,0,0));
  }
  for (i=0; i<map.length; i++) {
    for (n=0; n<map[i].length; n++) {
      var newdiv = document.createElement('div');
      newdiv.setAttribute('id', 'square'+i+'.'+n);
      newdiv.setAttribute('class', 'square empty');
      stage.appendChild(newdiv);
    }
  }
  
  block.create();
  
}

function fillMap() {
  for (i=0; i<block.coords.length; i++) {
    x = block.x+block.coords[i][0];
    y = block.y+block.coords[i][1];
    map[y][x] = 1;
    document.getElementById('square'+y+'.'+x).setAttribute('class', 'square full');
  }
}

function checkRows() {
  redraw = false;
  for (i=0; i<mapHeight; i++) {
    sum=0;
    for (n=0; n<map[i].length; n++) {
      sum += map[i][n];
    }
    if (sum == mapWidth) {
      map.splice(i, 1);
      map.unshift(new Array(0,0,0,0,0,0,0,0,0,0));
      redraw = true;
    }
  }
  if (redraw) redrawMap();
}

function redrawMap() {
  for (i=0; i<map.length; i++) {
    for (n=0; n<map[i].length; n++) {
      var elem = document.getElementById('square'+i+'.'+n);
      if (map[i][n] == 1)
        elem.setAttribute('class', 'square full');
      else
        elem.setAttribute('class', 'square empty');
    }
  }

}

window.onload = init();
document.onkeydown = keyListener;

function keyListener(e) {
  e = e || window.event;
  switch(e.keyCode) {
    case 38:
      block.rotate();
      break;
    case 37: 
      block.move(-1, 0, false);
      break;
    case 40:
      block.drop();
      break;
    case 39:
      block.move(1, 0, false);
      break;
  }
}
