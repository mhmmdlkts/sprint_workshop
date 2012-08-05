he = {}
preload = new PreloadJS()
fields = 
  b: "./images/block.jpg"
  s: "./images/stone.jpg"
  o: "./images/bomb.jpg"
  f: "./images/fire.jpg"
  g: "./images/grass.jpg"
  m: "./images/bomb.jpg"
  c: "./images/chara.png"
manifest = []
for key, val of fields
  manifest.push({src: val})
he.static = new Stage("static")
he.flow = new Stage('flow')
preload.loadManifest(manifest);
preload.onComplete = ->
  socket = io.connect('http://localhost:3000/');
  socket.on('stage init', (ini)->
    console.log(ini)
    he.static.objmap = []
    he.flow.objmap = []
    he.flow.chars = {}
    for y, row of ini.map
      he.static.objmap[y] ||= []
      for x, cell of row
        setPart(he.static, {x: x, y: y, type: cell})
    for y, row of ini.flow
      he.flow.objmap[y] ||= []
      for x, cell of row
        setPart(he.flow, {x: x, y: y, type: cell})
    for id, info of ini.char
      if typeof he.flow.chars[id] is 'undefined'
        setPart(he.flow, {x: info.x, y: info.y, type: 'c'}, id)
      else
        he.flow.chars[id].x = info.x*30
        he.flow.chars[id].y = info.y*30
    setTimeout(->
      he.static.update()
      he.flow.update()
    , 10)
  
  socket.on('stage sync', (diff) ->
    for info in diff.flow.add
      setPart(he.flow, {x: info.x, y: info.y, type: info.type})
    for info in diff.flow.delete
      delPart(he.flow, {x: info.x, y: info.y, type: info.type})
    
    for id, info of diff.char
      if typeof he.flow.chars[id] is 'undefined'
        setPart(he.flow, {x: info.x, y: info.y, type: 'c'}, id)
      else
        he.flow.chars[id].x = info.x*30
        he.flow.chars[id].y = info.y*30
    for id, obj of he.flow.chars
      if typeof diff.char[id] is 'undefined'
        he.flow.removeChild(he.flow.chars[id])
        delete(he.flow.chars[id])
    setTimeout(->
      he.flow.update()
    , 10)
  );
  window.document.onkeydown = (e) ->
    code = 's' if e.keyCode == 32
    code = 'l' if e.keyCode == 37
    code = 'u' if e.keyCode == 38
    code = 'r' if e.keyCode == 39
    code = 'd' if e.keyCode == 40
    socket.send(code)
  )

setPart = (canvas, opt, id) ->
  image = new Bitmap(fields[opt.type])
  image.width = image.height = 30
  image.x = opt.x * 30
  image.y = opt.y * 30
  if typeof id is 'undefined'
    canvas.objmap[opt.y][opt.x] = canvas.addChild(image)
  else
    canvas.chars[id] = canvas.addChild(image)
