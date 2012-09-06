package  {
    import flash.display.LineScaleMode;
    import flash.display.Sprite;
    import flash.events.MouseEvent;
    import flash.events.Event;
    [SWF(width=465 ,height=465,backgroundColor=0xFFFFFF,frameRate=60)]
    public class Main extends Sprite {

    private var mEnemyAppearCount:Number = 50;

        public function Main() {
            addEventListener(Event.ENTER_FRAME,update);
            this.stage.addEventListener( MouseEvent.CLICK,onMouseClick );
        }
        public function update(e:Event):void {
            --mEnemyAppearCount;
            if ( mEnemyAppearCount < 0 ) {
                mEnemyAppearCount = 60;
                var en : Enemy = new Enemy();
                mChara.push( en );
                stage.addChild( en );
            }
      
            for ( var i:Number = 0 ; i < mChara.length ; ) {
                mChara[ i ].update();
                if ( mChara[ i ].mDestroyed ) {
                    stage.removeChild( mChara[ i ] );
                    mChara.splice( i, 1 );
                }
                else {
                    ++i
                }
            }
        }
        public function onMouseClick(e:MouseEvent):void {
            var c : Canon = new Canon( mouseX,mouseY );
            mChara.push( c );
            stage.addChild( c );
        }
    }
}

import flash.display.Sprite;
import flash.display.GradientType;
import flash.geom.Matrix;

const StageWidth : int = 465;
const StageHeight : int = 465;

var mChara:Array = [];

class Chara extends Sprite {
    public var mDestroyed : Boolean = false;
        public function Chara( x:Number, y:Number ) {
        this.x = x;
        this.y = y;
    }
    public function collision( obj ) : Boolean {
        var u : Number = this.x - obj.x;
        var v : Number = this.y - obj.y;
        return u * u + v * v < 10 * 10;
    }
}

class Canon extends Chara {
    private var mWait : Number = 50;
    public function Canon( x : Number, y:Number ) {
        super(x, y);
        this.graphics.beginFill(0x00CCFF);
        this.graphics.drawCircle(0, 0, 10);
        this.graphics.endFill();
    }
    public function update() : void{
        --mWait;
        if ( mWait < 0 ) {
            mWait = 50;
          
            //敵をさがす。
            for each (var it in mChara) {
                if ( it.isEnemy() ) {
                    var d : Number = Math.atan2( it.y - this.y, it.x - this.x );
                    var s = new Shot( x, y, d );
                    mChara.push( s );
                    stage.addChild( s );
                    break;
                }
            }
        }
    }
    public function isEnemy() : Boolean {
        return false;
    }
}

class Shot extends Chara {
    private var mTime : Number = 60 * 10;
    private var mVelocityX : Number = 0;
    private var mVelocityY: Number = 1;
    public function Shot( X:Number, Y:Number, d:Number ) {
        super( X,Y );
        this.graphics.beginFill(0x0000FF);
        this.graphics.drawCircle(0, 0, 10);  
        this.graphics.endFill();
        mVelocityX = Math.cos( d );
        mVelocityY = Math.sin( d );
    }
    public function update() : void{
        this.x += mVelocityX;
        this.y += mVelocityY;
      
        --mTime;
        if ( mTime < 0 ) {
            mDestroyed = true;
        }
    }
    public function isEnemy() : Boolean {
        return false;
    }
}

class Enemy extends Chara {
    private var mTime : Number = 60 * 10;
    public function Enemy() {
        super( 0,  Math.random() * StageHeight );
        this.graphics.beginFill(0xFF0000);
        this.graphics.drawCircle(0, 0, 10);  
        this.graphics.endFill();
    }
    public function update() : void{
        //移動
        ++this.x;

        //玉との当たり判定
        for each (var it in mChara) {
            if ( !it.isEnemy() ) {
                if   (this.collision( it )) {
                    mDestroyed = true;
                    it.mDestroyed = true;
                }
            }
        }

        --mTime;
        if ( mTime < 0 ) {
            mDestroyed = true;
        }
    }
    public function isEnemy() : Boolean {
        return true;
    }
}

