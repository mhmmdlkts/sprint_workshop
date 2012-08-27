
const width = 400;
const height = 400;

var enemy = new Array;
var shot = new Array;
var canon = new Array;
var particle = new Array;

var enemyArray = new Array;
var generateEnemy = 0;

var score = 0;
var money = 700;
var life = 10;
var wave = 0;

var rail = null;

var weaponSwitchA = null;
var weaponSwitchB = null;
var weaponSwitchC = null;
var weaponSwitchD = null;
var currentWeapon = null;

var updateEvent = null;
var gameOver = false;

window.addEventListener('DOMContentLoaded',
  function () {
      if ( HTMLCanvasElement ) {
              var sc = document.querySelector('#screen');
              sc.addEventListener( "mousedown",mouseDown,false );
            sc.style.backgroundImage = "url(back.jpg)";
            
            weaponSwitchA = document.querySelector('#weaponA');
            weaponSwitchA.addEventListener( "click",selectWeaponA,false );
            weaponSwitchB = document.querySelector('#weaponB');
            weaponSwitchB.addEventListener( "click",selectWeaponB,false );
            weaponSwitchC = document.querySelector('#weaponC');
            weaponSwitchC.addEventListener( "click",selectWeaponC,false );
            weaponSwitchD = document.querySelector('#weaponD');
            weaponSwitchD.addEventListener( "click",selectWeaponD,false );

            selectWeaponA();

            rail = new Rail;
            rail.mPosition.push( new Vector2( 0,50 ) );
            rail.mPosition.push( new Vector2( 300,50 ) );
            rail.mPosition.push( new Vector2( 350,250 ) );
            rail.mPosition.push( new Vector2( 200,300 ) );
            rail.mPosition.push( new Vector2( 150,150 ) );
            rail.mPosition.push( new Vector2( 70,150 ) );
            rail.mPosition.push( new Vector2( 70,400 ) );
            
            rail.finalize();

            var c = sc.getContext('2d');
               c.fillStyle = "#ffffff";
            c.font = "italic 24px 'sans-serif'";
               c.textAlign = "center";
              c.fillText( "click" , width/2,height/2 );
      }
      else {
          window.alert(' canvas is not supported');
          return;
      }
  }, false
);

function update(){
    //敵出現パターン生成
    if ( enemyArray.length == 0 ){
        //出現する敵の種類
        var enemyList = null;
        if ( wave < 3 ) {
            enemyList = new Array( EnemyA );
        }
        else {
            enemyList = new Array( EnemyA,EnemyB );
        }
        var e = enemyList[ Math.floor( Math.random() * enemyList.length ) ];
    

        //まずインターバル
        for ( var i=0 ; i<5 ; ++i  ){
            enemyArray.push( null );
        }
        
        var n = wave + 5;
        if ( n > 30 ){
            n = 30;
        }
        for ( var i=0 ; i<n ; ++i  ){
            enemyArray.push( e );
        }
        for ( var i=0 ; i<5 ; ++i  ){
            enemyArray.push( null );
        }

        ++wave;
    }

    ++generateEnemy;
    if ( generateEnemy > 20 ) {
        generateEnemy = 0;
        if ( enemyArray[ 0 ] != null ){
            var e = new enemyArray[ 0 ];
               enemy.push( e );
             }
             enemyArray.shift();
       }
    
    for ( var i=0 ; i<canon.length ; ++i ) {
        canon[ i ].update();
    }
    for ( var i=0 ; i<enemy.length ; ++i ) {
        enemy[ i ].update();
        if ( enemy[ i ].destroyed() ) {
            if ( enemy[ i ].mHP < 1 ) {
                Explode( enemy[ i ].mSprite.mPosition )
                score += 100;
                money += 10;
            }
            else {
                if ( life > 0 ){
                    --life;
                    if ( life < 1 ) {
                        gameOver = true;
                    }
                }
            }
            delete enemy[ i ];
            enemy.splice( i,1 );
            --i;
        }
    }
    for ( var i=0 ; i<shot.length ; ++i ) {
        shot[ i ].update();
        if ( shot[ i ].destroyed() ){
            delete shot[ i ];
            shot.splice( i,1 );
            --i;
        }
    }
    for ( var i=0 ; i<particle.length ; ++i ) {
        particle[ i ].update();
        if ( particle[ i ].destroyed() ) {
            delete particle[ i ];
            particle.splice( i,1 );
            --i;
        }
    }
    var sc = document.querySelector('#screen');
    var c = sc.getContext('2d');
    c.clearRect(0, 0, width, height );

    //敵経路描画
    c.save();
    c.beginPath();
    c.strokeStyle = "#ffffff";
    c.lineWidth = 32;
    c.globalAlpha = 0.5;
    c.moveTo( rail.mPosition[ 0 ].x, rail.mPosition[ 0 ].y );
    for ( var i=1 ; i<rail.mPosition.length ; ++i ) {
        c.lineTo( rail.mPosition[ i ].x, rail.mPosition[ i ].y );
    }
    c.stroke();
    c.restore();
    
    for ( var i=0 ; i<canon.length ; ++i ) {
        canon[ i ].draw( c );
    }
    for ( var i=0 ; i<enemy.length ; ++i ) {
        enemy[ i ].draw( c );
    }
    for ( var i=0 ; i<shot.length ; ++i ) {
        shot[ i ].draw( c );
    }
    for ( var i=0 ; i<particle.length ; ++i ) {
        particle[ i ].draw( c );
    }
    
    document.querySelector('#score').innerHTML = score;
    document.querySelector('#money').innerHTML = money;
    document.querySelector('#wave').innerHTML = wave;
   document.querySelector('#life').innerHTML = life;
   
   if ( gameOver ) {
           clearInterval( updateEvent );
           c.fillStyle = "#ffffff";
        c.font = "italic 24px 'sans-serif'";
           c.textAlign = "center";
          c.fillText( "GameOver" , width/2,height/2 );
   }
}

function mouseDown( e ){
    if ( updateEvent == null) {
 　          updateEvent = setInterval( update, 1000/30 );
     }
    else {
        if ( currentWeapon.prototype.mCost <= money ){
            //既に置いてある砲台に被らないように。
            var rect = e.target.getBoundingClientRect();
            var pos = new Vector2( e.clientX - rect.left,e.clientY - rect.top );
            var find = false;
            for ( var i=0 ; i<canon.length ; ++i ){
                var dis = new Vector2( 0,0 );
                dis.add( pos );
                dis.sub( canon[ i ].mSprite.mPosition )
                if ( dis.squareLength() < 32*32 ){
                    find = true;
                    break;
                }
            }
            if ( !find ){
                var c = new currentWeapon();
    
                c.mSprite.mPosition = pos;
                canon.push( c );
        
                money -= c.mCost;
            }
        }
    }
}

function selectWeaponA( e ) {
    weaponSwitchA.style.backgroundColor = "#AAEEDD";
    weaponSwitchB.style.backgroundColor = "";
    weaponSwitchC.style.backgroundColor = "";
    weaponSwitchD.style.backgroundColor = "";
    currentWeapon = CanonA;
}
function selectWeaponB( e ) {
    weaponSwitchA.style.backgroundColor = "";
    weaponSwitchB.style.backgroundColor = "#AAEEDD";
    weaponSwitchC.style.backgroundColor = "";
    weaponSwitchD.style.backgroundColor = "";
    currentWeapon = CanonB;
}
function selectWeaponC( e ) {
    weaponSwitchA.style.backgroundColor = "";
    weaponSwitchB.style.backgroundColor = "";
    weaponSwitchC.style.backgroundColor ="#AAEEDD";
    weaponSwitchD.style.backgroundColor = "";
    currentWeapon = CanonC;
}
function selectWeaponD( e ) {
    weaponSwitchA.style.backgroundColor = "";
    weaponSwitchB.style.backgroundColor = "";
    weaponSwitchC.style.backgroundColor = "";
    weaponSwitchD.style.backgroundColor = "#AAEEDD";
    currentWeapon = CanonD;
}

function Sprite() {
    this.mPosition = new Vector2( 0,0 );
    this.mVelocity = new Vector2( 0,0 );
    this.mAccel = new Vector2( 0,0 );
};

Sprite.prototype = {
    mPosition : null,
    mVelocity : null,
       mAccel: null,
    mRotate:0,
    mImage:null,
    mShadow :false,
    mSize : 16,
    mCompositeOperation : null,
    mAlpha : 1,
    update:function(){
        this.mPosition.add( this.mVelocity );
        this.mVelocity.add( this.mAccel );
    },
    draw:function( c ){
        c.save();
        if ( this.mShadow ) {
            c.shadowColor = "#000000";
            c.shadowBlur = 10;
            c.shadowOffsetX = 5;
            c.shadowOffsetY = 5;
        }
        if ( this.mCompositeOperation != null ) {
            c.globalCompositeOperation = this.mCompositeOperation;
        }
        if ( this.mAlpha != 1 ) {
            c.globalAlpha = this.mAlpha;
        }
        c.translate( this.mPosition.x,this.mPosition.y );
        c.rotate( this.mRotate );
        if ( this.mImage == null ){
            c.fillRect( -this.mSize / 2, -this.mSize / 2, this.mSize, this.mSize );
        }
        else {
            c.drawImage( this.mImage,-this.mSize / 2,-this.mSize / 2,this.mSize,this.mSize );
        }
        c.restore();
    },
    isOutOfScreen:function(){
        return this.mPosition.x < 0 || this.mPosition.y < 0 || this.mPosition.x > width || this.mPosition.y > height; 
    },
    collide:function( obj ){
        var dis = new Vector2( 0,0 );
        dis.add( this.mPosition );
        dis.sub( obj.mPosition );
        var r = ( this.mSize + obj.mSize ) / 2;
        return dis.squareLength() < r*r;
    }
}

function Enemy() {
    this.mSprite = new Sprite;
    this.mSprite.mShadow = true;
    this.mSprite.mSize = 32;
}

Enemy.prototype = {
    mSprite : null,
    mCount : 0,
    mStun : 0,
    draw : function( ctx ) {
        this.mSprite.draw( ctx );
    },
    update : function() {
        if ( this.mStun > 0 ){
            --this.mStun;
        }
        else {
            var p = rail.getPosition( this.time() );
            if ( p != null ) {
                this.mSprite.mRotate = Math.atan2( p.y - this.mSprite.mPosition.y , p.x - this.mSprite.mPosition.x );
                this.mSprite.mPosition = p;
            }
            ++ this.mCount;
        }
    },
    destroyed : function() {
        return this.mHP < 1 || this.time() > 1;
    },
    time : function() {
        return this.mCount * this.mVelocity / rail.mLength;
    }
}

function EnemyA() {
    Enemy.call( this );
    this.mVelocity = 2;
    this.mHP = 10 + wave;
    this.mSprite.mImage = new Image;
    this.mSprite.mImage.src = "enemy.png";
}
EnemyA.prototype = new Enemy;

function EnemyB() {
    Enemy.call( this );
    this.mVelocity = 3.5;
    this.mHP = 5 + Math.floor( wave / 2 );
    this.mSprite.mImage = new Image;
    this.mSprite.mImage.src = "enemy2.png";
}
EnemyB.prototype = new Enemy;


function Canon() {
    this.mSprite = new Sprite;
    this.mSprite.mShadow = true;
    this.mSprite.mSize = 32;
}

Canon.prototype = {
    mSprite : null,
    mAngle : 0,
    mCharge : 0,
    draw : function( ctx ) {
        this.mSprite.draw( ctx );
    },
    update : function() {
        ++this.mCharge;
    
        //一番近い敵をさがす
        var nearestEnemy = null;
        var squareDistance = 0;
        for( var i = 0 ; i<enemy.length ; ++i ) {
            var dis = new Vector2( 0,0 );
            dis.add( this.mSprite.mPosition );
            dis.sub( enemy[ i ].mSprite.mPosition );
            if ( nearestEnemy == null || squareDistance > dis.squareLength() ) {
                nearestEnemy = enemy[ i ];
                squareDistance = dis.squareLength();
            }
        }
        if ( nearestEnemy != null ) {

            var v1 = new Vector2( 0,0 );
            v1.add( nearestEnemy.mSprite.mPosition );
            v1.sub( this.mSprite.mPosition );

            if ( v1.squareLength() < this.mRange * this.mRange ) {
                var v2 = new Vector2( Math.cos( this.mSprite.mRotate ),Math.sin( this.mSprite.mRotate ) );
                var ang = Math.atan2( v1.cross( v2 ),v1.dot( v2 ) );
                var rotateV = 4 * Math.PI / 180;
                if ( ang > rotateV ) {
                    this.mSprite.mRotate += -rotateV;
                }
                else if ( ang < -rotateV  ){
                    this.mSprite.mRotate += rotateV;
                }
                else {
                    this.mSprite.mRotate -= ang;
                }
                var attackAng = 15 * Math.PI / 180;
                if ( ang*ang < attackAng*attackAng ){

                    if ( this.mCharge >  this.mChargeTime ){
                        this.mCharge = 0;
                        var s = new this.mShot();
                        s.mSprite.mPosition.set( this.mSprite.mPosition );
                        s.mSprite.mVelocity = new Vector2( s.mVelocity * Math.cos( this.mSprite.mRotate ),s.mVelocity * Math.sin( this.mSprite.mRotate ) );
                        s.mSprite.mRotate = this.mSprite.mRotate
                        shot.push( s );
                    }
                }
            }
        }
        this.mSprite.update();
    }
}

function Shot() {
    this.mSprite = new Sprite;
    this.mExplode = SmallExplode;
    this.mSprite.mImage = new Image;
    this.mSprite.mImage.src = "shot.png";
    this.mSprite.mSize = 16;
}

Shot.prototype = {
    mSprite : null,
    mDestroyed : false,
    mExplode : false,
    draw : function( ctx ) {
        this.mSprite.draw( ctx );
    },
    update : function() {
        this.mSprite.update();
        for( var i=0 ; i<enemy.length ; ++i ) {
            if ( this.mSprite.collide( enemy[ i ].mSprite ) ) {
                enemy[ i ].mHP -= this.mAttack;
                if ( enemy[ i ].mStun == 0 ) {
                    enemy[ i ].mStun = this.mStun;
                }
                this.mDestroyed = true;
                
                this.mExplode( this.mSprite.mPosition );
            }
        }
    },
    destroyed : function(){
        return this.mDestroyed || this.mSprite.isOutOfScreen();
    }
}

function CanonA() {
    Canon.call( this );
    this.mChargeTime = 10;
    this.mShot = ShotA;
    this.mRange = 120;
    this.mSprite.mImage = new Image;
    this.mSprite.mImage.src = "c1.png";
}
CanonA.prototype = new Canon;
CanonA.prototype.mCost = 100;

function ShotA() {
    Shot.call( this );
    this.mAttack = 1;
    this.mStun = 0;
    this.mVelocity = 4;
}
ShotA.prototype = new Shot;

function CanonB() {
    Canon.call( this );
    this.mChargeTime = 20;
    this.mShot = ShotB;
    this.mRange = 120;
    this.mSprite.mImage = new Image;
    this.mSprite.mImage.src = "c2.png";
}
CanonB.prototype = new Canon;
CanonB.prototype.mCost = 100;

function ShotB() {
    Shot.call( this );
    this.mAttack = 2;
    this.mStun = 0;
    this.mVelocity = 4;

}
ShotB.prototype = new Shot;



function CanonC() {
    Canon.call( this );
    this.mChargeTime = 25;
    this.mShot = ShotC;
    this.mRange = 200;
    this.mSprite.mImage = new Image;
    this.mSprite.mImage.src = "c3.png";
}
CanonC.prototype = new Canon;
CanonC.prototype.mCost = 100;

function ShotC() {
    Shot.call( this );
    this.mAttack = 1;
    this.mStun = 0;
    this.mVelocity = 8;
}
ShotC.prototype = new Shot;



function CanonD() {
    Canon.call( this );
    this.mChargeTime = 30;
    this.mShot = ShotD;
    this.mRange = 120;
    this.mSprite.mImage = new Image;
    this.mSprite.mImage.src = "c4.png";

}
CanonD.prototype = new Canon;
CanonD.prototype.mCost = 100;

function ShotD() {
    Shot.call( this );
    this.mAttack = 1;
    this.mStun = 5;
    this.mVelocity = 4;
}
ShotD.prototype = new Shot;

function Particle() {
    this.mSprite = new Sprite;
    this.mSprite.mImage = new Image;
    this.mSprite.mImage.src = "enemyExplode.png";
    this.mSprite.mCompositeOperation = "lighter";
}

Particle.prototype = {
    mSprite : null,
    mCount : 0,
    draw : function( ctx ) {
        this.mSprite.draw( ctx );
    },
    update : function() {
        ++this.mCount;
        this.mSprite.update();
        this.mSprite.mVelocity.mul( 0.95 );
        this.mSprite.mAlpha *= 0.90;
    },
    destroyed : function(){
        return this.mCount > 60;
    }
}

function Explode( pos ){
    for ( var w = 0 ; w<50 ; ++w ) {
        var p = new Particle();
        particle.push( p );
        p.mSprite.mPosition.set( pos );
        var ang = Math.random() * Math.PI * 2;
        p.mSprite.mVelocity= new Vector2( Math.cos( ang ) ,Math.sin( ang ) );
        p.mSprite.mVelocity.mul( Math.random() * 4 );
        p.mSprite.mRotate = ang;
    }    
}


function SmallExplodeParticle() {
    this.mSprite = new Sprite;
    this.mSprite.mImage = new Image;
    this.mSprite.mImage.src = "explode.png";
    this.mSprite.mCompositeOperation = "lighter";
    this.mSprite.mSize = 32;
}

SmallExplodeParticle.prototype = {
    mSprite : null,
    mCount : 0,
    draw : function( ctx ) {
        this.mSprite.draw( ctx );
    },
    update : function() {
        ++this.mCount;
        this.mSprite.update();
        this.mSprite.mAlpha *= 0.90;
    },
    destroyed : function(){
        return this.mCount > 4;
    }
}

function SmallExplode( pos ){
    for ( var w = 0 ; w<1 ; ++w ) {
        var p = new SmallExplodeParticle();
        particle.push( p );
        p.mSprite.mPosition.set( pos );
    }    
}

function Vector2( x,y ) {
    this.x = x;
    this.y = y;
}

Vector2.prototype = {
    x : 0,
    y : 0,
    set : function( v ){
        this.x = v.x;
        this.y = v.y;
    },
    add : function( v ){
        this.x += v.x;
        this.y += v.y;
    },
    sub : function( v ){
        this.x -= v.x;
        this.y -= v.y;
    },
    mul : function( a ){
        this.x *= a;
        this.y *= a;
    },
    cross : function( v ){
        return this.x*v.y - this.y*v.x;
    },
    dot : function( v ){
        return this.x*v.x + this.y*v.y;
    },
    squareLength : function() {
        return this.x*this.x + this.y*this.y;
    },
    length : function() {
        return Math.sqrt( this.squareLength() );
    }
}

function lerp( t,A,B ){
    return t*A + ( 1-t )*B;
}
function lerpVector2( t,A,B ){
    var result = new Vector2(
        lerp( t,A.x,B.x ),
        lerp( t,A.y,B.y ) );
    return result;
}


function Rail() {
    this.mPosition = new Array;
}

Rail.prototype = {
    mPosition : null,
    mTime : null,
    mLength : 0,
    finalize : function(){
        //まず全距離を計算
        this.mLength = 0;
        var length = new Array;
        for ( var i=0 ; i<this.mPosition.length - 1 ; ++i ){
            var dis = new Vector2( 0,0 );
            dis.add( this.mPosition[ i+1 ] );
            dis.sub( this.mPosition[ i ] );
            var L = dis.length();
            length.push( L );
            this.mLength += L;
        }
        this.mTime = new Array;
        //時間計算
        var t = 0;
        this.mTime.push( 0 );
        for ( var i=0 ; i<length.length; ++i ){
            t += length[ i ] / this.mLength;
            this.mTime.push( t );
        }
    },
    getPosition : function( t ) {
        for ( var i=1 ; i<this.mTime.length ; ++i ){
            if ( this.mTime[ i ] > t ){
                var p = ( t - this.mTime[ i - 1 ]) / ( this.mTime[ i ] - this.mTime[ i - 1 ] );
                return lerpVector2( p,this.mPosition[ i ],this.mPosition[ i - 1 ] );
            }
        }
        return null;    
    }
}
    


