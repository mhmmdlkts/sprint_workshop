var colors = new Array('red', 'blue', 'yellow');
var players = new Array(
    {
      'image': 'blue', 
      'red': 60,
      'yellow': 60,
      'blue': 80,
      'energy': 100,
      'cooldown': 0,
      'energy_cost': 40,
      'energy_gain': 10,
      'recovery': 50
    },
    {
      'image': 'pink', 
      'red': 40,
      'yellow': 120,
      'blue': 50,
      'energy': 100,
      'cooldown': 0,
      'energy_cost': 40,
      'energy_gain': 10,
      'recovery': 50
    },
    {
      'image': 'red', 
      'red': 80,
      'yellow': 80,
      'blue': 50,
      'energy': 100,
      'cooldown': 0,
      'energy_cost': 40,
      'energy_gain': 10,
      'recovery': 50
    },
    {
      'image': 'red', 
      'red': 80,
      'yellow': 50,
      'blue': 80,
      'energy': 100,
      'cooldown': 0,
      'energy_cost': 40,
      'energy_gain': 10,
      'recovery': 50
    },
    {
      'image': 'blue', 
      'red': 50,
      'yellow': 50,
      'blue': 100,
      'energy': 100,
      'cooldown': 0,
      'energy_cost': 40,
      'energy_gain': 10,
      'recovery': 50
    }
  );
var tension = {
  'red': {
    'max': 300,
    'current': 0
  },
  'yellow': {
    'max': 240,
    'current': 0
  },
  'blue': {
    'max': 260,
    'current': 0
  }
}
var stageRight = new Array();
var stageLeft = new Array();
var choosingFirst = true;
var leftActor = -1;
var rightActor = -1;
var centerActor = -1;
var score = 0;
var currentSong = 1;
var currentTurn = 1;
var maxSong = 3;
var maxTurn = 5;
var feverNow = false;

function init() {
  for (i=0; i<3; i++) {
    stageRight.push(colors[Math.floor(Math.random()*3)]);
    stageLeft.push(colors[Math.floor(Math.random()*3)]);
  }
  for (i=0; i<5; i++) {
    $("#actor"+i).click(function() {
      appeal(this.id.replace('actor', ''));
    });
    $("#actor"+i).css('background-image', "url('images/actor/" + players[i]['image'] + ".png')");
    text = "<span style='color:red'>★</span> ";
    text += players[i]['red'];
    text += "<br />";
    text += "<span style='color:yellow'>★</span> ";
    text += players[i]['yellow'];
    text += "<br />";
    text += "<span style='color:#3580fe'>★</span> ";
    text += players[i]['blue'];
    text += "<br />";
    $("#params"+i).html(text);
  }
}

function resetStage() {
  for (i=0; i<5; i++) {
    $("#actor"+i).removeClass('disabled');
  }
  $("#black").css('display', 'none');
  $("#center-spot").css('display', 'none');
  $("#leftactor").css('background-image', '');
  $("#rightactor").css('background-image', '');
  $("#centeractor").css('background-image', '');
  $("#leftscore").html('');
  $("#rightscore").html('');
  $("#centerscore").html('');
  $("#red-tension").css('width', tension['red'].max * 0.5 + "px");
  $("#yellow-tension").css('width', tension['yellow'].max * 0.5 + "px");
  $("#blue-tension").css('width', tension['blue'].max * 0.5 + "px");
  $("#red-tension .bar").css('width', tension['red'].current * 0.5 + "px");
  $("#yellow-tension .bar").css('width', tension['yellow'].current * 0.5 + "px");
  $("#blue-tension .bar").css('width', tension['blue'].current * 0.5 + "px");
  $("#turn").html(currentSong + '曲目: ' + currentTurn + '/5')
  choosingFirst = true;
  leftActor = rightActor = -1
  moveCircles();
  redraw();
}

function redraw() {
  for (i=0; i<3; i++) {
    $("#rightcircle"+i).find('img')[0].src = "images/circle/"+stageRight[i]+".png";
    $("#leftcircle"+i).find('img')[0].src = "images/circle/"+stageLeft[i]+".png";
  }
  for (i=0; i<5; i++) {
    $("#actor"+i+" .cooldown").css('height', players[i].cooldown+"%");
    $("#actor"+i+" .energy .bar").css('width', players[i].energy+"%");
  }
  $("#spot1").css('background-image', "url('images/spot/right-" + stageRight[0] + ".png')");
  $("#spot0").css('background-image', "url('images/spot/left-" + stageLeft[0] + ".png')");
}

function moveCircles() {
  stageRight.shift();
  stageLeft.shift();
  stageRight.push(colors[Math.floor(Math.random()*3)]);
  stageLeft.push(colors[Math.floor(Math.random()*3)]);
}

function updatePlayers() {
  for (i=0; i<5; i++) {
    players[i].cooldown -= players[i].recovery;
    if (players[i].cooldown <= 0) {
      if (players[i].energy == 0) {
        players[i].cooldown = 0;
        players[i].energy = 100;
      } else {
        players[i].energy = Math.min(players[i].energy + players[i].energy_gain, 100);
      }
    }
  }
  resetStage();
}

function fever() {
  feverNow = true
  $("#fever").css('display', 'block')
  $("#black").css('display', 'block')
  $("#center-spot").css('display', 'block')

}

function finishTurn() {
  if (currentTurn >= maxTurn) {
    if (currentSong >= maxSong) {
      finish();
    } else {
      currentTurn = 0;
      currentSong++;
      tension['red'].current *= 0.8
      tension['yellow'].current *= 0.8
      tension['blue'].current *= 0.8
    }
  }
  feverNow = false

  setTimeout(function(){$("#fever").css('display', 'none')}, 1000);
  setTimeout(function(){updatePlayers()}, 1000);
}

function getScore() {
  currentTurn++;
  leftScore = players[leftActor][stageLeft[0]];
  rightScore = players[rightActor][stageRight[0]];
  $("#leftscore").html(leftScore);
  $("#rightscore").html(rightScore);
  score += leftScore + rightScore;
  $("#score").html(score);

  if (leftScore + tension[stageLeft[0]].current > tension[stageLeft[0]].max) {
    leftOver = leftScore + tension[stageLeft[0]].current - tension[stageLeft[0]].max
    colors.forEach(function(color) {
      if (stageLeft[0] != color) tension[color].current = Math.min(tension[color].current + leftOver * 0.3, tension[color].max)
    });
    tension[stageLeft[0]].current = tension[stageLeft[0]].max;
  } else {
    tension[stageLeft[0]].current += leftScore;
  }

  if (rightScore + tension[stageRight[0]].current > tension[stageRight[0]].max) {
    leftOver = tension[stageRight[0]].max - tension[stageRight[0]].current;
    colors.forEach(function(color) {
      if (stageRight[0] != color) tension[color].current = Math.min(tension[color].current + leftOver * 0.3, tension[color].max)
    });
    tension[stageRight[0]].current = tension[stageRight[0]].max;
  } else {
    tension[stageRight[0]].current += rightScore;
  }

  if (tension['red'].current == tension['red'].max && tension['yellow'].current == tension['yellow'].max && tension['blue'].current == tension['blue'].max) {
    fever();
  } else {
    finishTurn();
  } 
}

function showFeverCutin(score) {
  $("#fever-score").html(score + 'pt BONUS');
  $("#cutin").css('display', 'block');
  $("#cutin-actors").addClass('animate');
  setTimeout(function(){$("#cutin").css('display', 'none');}, 1500);
}

function getFeverScore() {
  centerScore = players[centerActor][stageLeft[0]] + players[centerActor][stageRight[0]]
  $("#centerscore").html(centerScore);
  feverScore = 0.5 * (leftScore + centerScore + rightScore)
  score += centerScore + feverScore
  $("#score").html(score);
  showFeverCutin(feverScore);
  tension['red'].current = tension['yellow'].current = tension['blue'].current = 0
  setTimeout(function(){finishTurn();}, 1500);
}


function appeal(id) {
  if (players[id].energy <= 0) return false
  if (leftActor == id || rightActor == id) return false
  
  players[id].energy = Math.max(players[id].energy - players[id].energy_cost, 0)
  if (players[id].energy <= 0) players[id].cooldown = 100 + players[id].recovery;
  actor = $("#actor"+id);
  if (feverNow) {
    $("#centeractor").css('background-image', actor.css('background-image'));
    actor.addClass('disabled');
    centerActor = id;
    getFeverScore();
  } else if (choosingFirst) {
    $("#leftactor").css('background-image', actor.css('background-image'));
    actor.addClass('disabled');
    leftActor = id;
    choosingFirst = false;
  } else {
    $("#rightactor").css('background-image', actor.css('background-image'));
    actor.addClass('disabled');
    rightActor = id;
    getScore();
  }
}

function finish(){
  alert(score)
}


window.onload = function() {
  init();
  resetStage();
}