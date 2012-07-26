enchant();

window.onload = function() {
    var game = new Game(320, 320);
    var ball;
    var paddle;
    var blockCount = 0;
    game.fps = 24;
    game.preload('http://jsrun.it/assets/j/T/K/8/jTK8u.png', 'http://jsrun.it/assets/s/I/a/3/sIa36.png', 'http://jsrun.it/assets/j/7/V/I/j7VIu.png', 'http://jsrun.it/assets/i/L/t/n/iLtnM.png');
    // The images used in the game should be preloaded

    game.onload = function() {
        
        for (var n=0; n<4; n++) {
            for (var i=0; i<7; i++) {
                var block = new Sprite(40, 10);
                block.x = 20 + 40*i;
                block.y = 20 + 10*n;
                block.image = game.assets['http://jsrun.it/assets/j/7/V/I/j7VIu.png'];
                game.rootScene.addChild(block);
                blockCount++;
                block.addEventListener('enterframe', function(e) {
                	if(ball.intersect(this)) {
                        ball.yDir *= -1;
                        ball.y += ball.yDir * 2;
  						game.rootScene.removeChild(this);
                        blockCount--;
                        if (blockCount == 0)
                            game.stop();
                        if (Math.random() > 0.7) 
                            addPowerup();
					}  
                });
            }
        }
        paddle = new Sprite(100, 10);
        paddle.x = 110;
        paddle.y = 270;
        paddle.image = game.assets['http://jsrun.it/assets/s/I/a/3/sIa36.png'];
        
        ball = new Sprite(30, 30);
        ball.x = 145;
        ball.y = 200;
        ball.xDir = 3;
        ball.yDir = -5;
        ball.image = game.assets['http://jsrun.it/assets/j/T/K/8/jTK8u.png'];

        ball.addEventListener('enterframe', function(e) {
            ball.x += ball.xDir;
            ball.y += ball.yDir;
            ball.rotate(ball.xDir*3);
                    
            // Check for world bounds
            if (ball.x < 0) {
            	ball.xDir *= -1;
                ball.x = 0;
            } else if (ball.x > game.width - ball.width) {
             	ball.xDir *= -1;
                ball.x = game.width - ball.width;
            } else if (ball.y < 0) {
            	ball.yDir *= -1;
                ball.y = 0;
            } else if (ball.y > game.height - ball.height) {
             	game.stop();
            }  
        });
        
        paddle.addEventListener('enterframe', function(e) {
         	if (game.input.right) {
                paddle.x += 8;
                // move position
            }
            if (game.input.left) {
                paddle.x -= 8;
            }   
            // Check for collision
            if(ball.intersect(paddle)) {
                if (ball.x - paddle.x < 0.25 * paddle.width) {
                    ball.xDir -= 1.5;
                } else if (ball.x - paddle.x > 0.75 * paddle.width) {
                    ball.xDir += 1.5;
                } 
  				ball.yDir = -1 * (Math.abs(ball.yDir) + 0.25);
			}  
        });
              
        var pad = new Pad();
        pad.x = 0;
        pad.y = 150;

        function addPowerup() {
        	var powerup = new Sprite(30, 30);
            powerup.x = ball.x;
            powerup.y = ball.y;
            powerup.image = game.assets['http://jsrun.it/assets/i/L/t/n/iLtnM.png'];
            powerup.addEventListener('enterframe', function(e) {
            	powerup.y += 3;
                if(this.intersect(paddle)) {
                    game.rootScene.removeChild(this);
                 	switch(Math.floor(Math.random()*2)) {
                     	case 0: 
                            paddle.width += 20;
                            break;
                        case 1:
                           	paddle.width -= 20;
                            break;
                    }
                }
            });
            game.rootScene.addChild(powerup);
        }
        
        // add sprites to rootScene (default scene)
        game.rootScene.addChild(ball);
        game.rootScene.addChild(paddle);
        game.rootScene.addChild(pad);
        game.rootScene.backgroundColor = '#ffffff';
    };
    game.start();
};
