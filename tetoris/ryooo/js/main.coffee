el = document.getElementById('mainsvg');
sc = document.getElementById('score');

r1 = Raphael(el, 550, 400);
CELL=20
CELL_BK='gray'
CELL_KEEP='blue'

class Map
  constructor:(x, y, width, height) ->
    @arr = []
    @arrWk = []
    @currentBlock = null
    @pt = 0
    @x = x
    @y = y
    for i in [0...height]
      @arr[i] = @createRow(width)
  
  createRow:(width = 10) ->
    arr = []
    for j in [0...width]
      arr[j] = CELL_BK
    return arr
  
  putBlock: (block)->
    @currentBlock = block
    @arrWk = []
    arr = block.current()
    for cell in arr
      x = cell[0] + block.x
      y = cell[1] + block.y
      @arrWk[y] ||= []
      @arrWk[y][x] = block.color
  
  keepBlock: (block)->
    @currentBlock = null
    @arrWk = []
    arr = block.current()
    for cell in arr
      x = cell[0] + block.x
      y = cell[1] + block.y
      @arr[y] ||= []
      @arr[y][x] = CELL_KEEP
  
  checkBlock: (block)->
    arr = block.current()
    for cell in arr
      x = cell[0] + block.x
      y = cell[1] + block.y
      return false unless @arr[y]
      return false if @arr[y][x] != CELL_BK
    return true
  
  checkRows: ->
    completeRows = []
    for row, i in @arr
      flg = true
      for cell, j in row
        flg = false if @arr[i][j] == CELL_BK
      if flg == true
        completeRows.push(i)
    
    # 削除
    # completeRows = completeRows.reverse()
    for i in completeRows
      # 対象行を削除
      @arr.splice(i,1)
      # 先頭に空行追加
      row = @createRow()
      @arr.unshift(row)
      @pt += 1
  
  render:(r = r1) ->
    @checkRows()
    sc.innerText = @pt
    for row, i in @arr
      for cell, j in row
        if @arrWk[i] && @arrWk[i][j]
          fillCell = @arrWk[i][j]
        else
          fillCell = cell
        r.rect(@x + CELL*j, @y + CELL*i, CELL, CELL).attr({ fill: fillCell, stroke: '#888888'})

class Block
  conf:[
    [[[0,0],[0,1],[1,0],[1,1]]],
    #[[[0,0],[0,1],[1,0],[0,2]], [[0,0],[0,1],[1,1],[2,1]], [[1,0],[1,1],[1,2],[0,2]], [[0,1],[1,1],[2,1],[2,2]]],
    [[[0,0],[0,1],[0,2],[0,3]], [[0,0],[1,0],[2,0],[3,0]]],
  ]
  constructor:(x = 4, y = 0) ->
    @x = x
    @y = y
    @xOrg = @yOrg = @rotate = @rotateOrg = 0
    @arr = []
    @color = "#f00"
    
    idx = Math.floor(Math.random()*@conf.length)
    @arr = @conf[idx]
  
  current: ->
    idx = @rotate % @arr.length
    return @arr[idx]
  
  move:(x, y) ->
    @keepOrg()
    @x += x
    @y += y
    return @
  
  turn: ->
    @keepOrg()
    @rotate += 1
    return @
  
  keepOrg: ->
    @rotateOrg = @rotate
    @xOrg = @x
    @yOrg = @y
    
  rollback: ->
    @rotate = @rotateOrg
    @x = @xOrg
    @y = @yOrg
    return @
  


map = new Map(0, 0, 10, 20)
next = new Map(220, 0, 5, 5)
block = new Block(2, 1)
next.putBlock(block)
next.render()



id = setInterval(->
  if map.currentBlock == null
    
    block = next.currentBlock
    block.x = 4
    next.currentBlock = null
    next.arrWk = []
    next.putBlock(new Block(2, 1))
    next.render()
    
    unless map.checkBlock(block)
      sc.innerText = map.pt+' (You dead)'
      clearInterval(id)
      return false
    else
      map.putBlock(block)
  else
    block = map.currentBlock.move(0,1)
    if map.checkBlock(block)
      map.putBlock(block)
    else
      block = map.currentBlock.move(0,-1)
      map.keepBlock(block)
  
  map.render()
, 100)
map.render()

window.document.onkeydown = (e) ->
  x = y = 0
  org = map.currentBlock
  if e.keyCode == 32
    block = map.currentBlock.turn()
  else
    x = -1 if e.keyCode == 37
    x = 1 if e.keyCode == 39
    y = 1 if e.keyCode == 40
    if map.currentBlock
      block = map.currentBlock.move(x, y)
  
  if block
    if map.checkBlock(block)
      map.putBlock(block)
    else
      if map.currentBlock
        block = map.currentBlock.rollback()
        if e.keyCode == 40
          map.keepBlock(block)
    map.render()



