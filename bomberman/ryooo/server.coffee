
class Stage
  constructor: ->
    @map = []
    @flow = []
    @char = {}
    @diffA = []
    @diffD = []
    @charCanStay = []
    for y in [0..20]
      @map[y] = []
      @flow[y] = []
      for x in [0..20]
        @map[y][x] ||= null
        @flow[y][x] ||= null
        if (x % 2 is 1) and (y % 2 is 1)
          @map[y][x] = 's'
        else
          @map[y][x] = 'g'
          if Math.random() <= 0.3
            @flow[y][x] = 'b'
          else
            @charCanStay.push({x:x, y:y})
  bornChar: (id) ->
    pos = @charCanStay[parseInt(Math.random()*@charCanStay.length)]
    @char[id] = new Char(@, pos.x, pos.y)
  deadChar: (id) ->
    delete(@char[id])
  isEmpty: (x, y) ->
    return false if @map[y][x] in ['s']
    return false if @flow[y][x] in ['b', 'o']
    return true
  getChar: -> 
    ret = {}
    for id, obj of @char
      ret[id] = {x: obj.x, y: obj.y}
    return ret
  getFlowDiff: ->
    ret = {add: @diffA, delete: @diffD}
    @diffA = []
    @diffD = []
    return ret
  putBomb: (x, y) ->
    @flow[y][x] = 'o'
    @diffA.push({x: x, y: y, type: 'o'})

class Char
  constructor: (stage, x, y)->
    @stage = stage
    @x = x
    @y = y
    @bomb = 0
  move: (x, y) ->
    if @stage.isEmpty(@x+x, @y+y) && @x+x <= 20 && @y+y <= 20 && 0 <= @x+x && 0 <= @y+y
      @x += x
      @y += y
  putBomb: ->
    if @bomb <= 3
      @stage.putBomb(@x, @y)

global.stage = new Stage()

socketio = require('socket.io').listen(global.app)
socketio.on('connection', (socket)->
  socket.on('message', (val)->
    char = global.stage.char[socket.id]
    return if typeof char is 'undefined'
    char.move(-1, 0) if val is 'l'
    char.move(1, 0)  if val is 'r'
    char.move(0, -1) if val is 'u'
    char.move(0, 1)  if val is 'd'
    char.putBomb() if val is 's'
  )
  socket.on('disconnect', (v) ->
    global.stage.deadChar(socket.id)
  )
  
  if typeof global.timer is 'undefined'
    global.timer = setInterval(->
      diff = 
      socket.broadcast.emit('stage sync', {flow:global.stage.getFlowDiff(), char: global.stage.getChar()});
    , 100)
  
  charId = global.stage.bornChar(socket.id)
  socket.emit('stage init', { map: global.stage.map, flow: global.stage.flow, char: global.stage.getChar()});
)
