#include "HelloWorldScene.h"

USING_NS_CC;

CCScene* HelloWorld::scene()
{
    // 'scene' is an autorelease object
    CCScene *scene = CCScene::create();
    
    // 'layer' is an autorelease object
    HelloWorld *layer = HelloWorld::create();

    // add layer as a child to scene
    scene->addChild(layer);

    // return the scene
    return scene;
}

// on "init" you need to initialize your instance
bool HelloWorld::init()
{
    //////////////////////////////
    // 1. super init first
    if ( !CCLayer::init() )
    {
        return false;
    }
    
    CCSize visibleSize = CCDirector::sharedDirector()->getVisibleSize();
    CCPoint origin = CCDirector::sharedDirector()->getVisibleOrigin();

    /////////////////////////////
    // 2. add a menu item with "X" image, which is clicked to quit the program
    //    you may modify it.

    // add a "close" icon to exit the progress. it's an autorelease object
    CCMenuItemImage *pCloseItem = CCMenuItemImage::create(
                                        "CloseNormal.png",
                                        "CloseSelected.png",
                                        this,
                                        menu_selector(HelloWorld::menuCloseCallback));
    
	pCloseItem->setPosition(ccp(origin.x + visibleSize.width - pCloseItem->getContentSize().width/2 ,
                                origin.y + pCloseItem->getContentSize().height/2));

    // create menu, it's an autorelease object
    CCMenu* pMenu = CCMenu::create(pCloseItem, NULL);
    pMenu->setPosition(CCPointZero);
    this->addChild(pMenu, 1);

    /////////////////////////////
    // 3. add your codes below...

    // add a label shows "Hello World"
    // create and initialize a label
    
    mLabel = CCLabelTTF::create("かわす", "Arial", 48);
    
    // position the label on the center of the screen
    mLabel->setPosition(ccp(origin.x + visibleSize.width/2,
                            origin.y + visibleSize.height - mLabel->getContentSize().height));

    // add the label as a child to this layer
    this->addChild(mLabel, 1);

    // add "HelloWorld" splash screen"
    CCSprite* pSprite = CCSprite::create("HelloWorld.png");

    // position the sprite on the center of the screen
    pSprite->setPosition(ccp(visibleSize.width/2 + origin.x, visibleSize.height/2 + origin.y));

    // add the sprite as a child to this layer
    this->addChild(pSprite, 0);

    mSprites.push_back(Sprite());
    mPlayer = &mSprites.back();
    mPlayer->mX = visibleSize.width/2 + origin.x;
    mPlayer->mY = visibleSize.height/2 + origin.y;
    mPlayer->mSprite = CCSprite::create("blocks.png",CCRect(32,0,32,32));
    this->addChild(mPlayer->mSprite, 0);
    mPlayer->mSprite->setZOrder(2);
    mPlayer->mResist = 1.f;

    this->scheduleUpdate();

    setAccelerometerEnabled(true);
    setAccelerometerInterval(1.0f / 60.0f);

    mTimeCount = 0.f;
    mWave = 0;

    return true;
}


void HelloWorld::menuCloseCallback(CCObject* pSender)
{
    CCDirector::sharedDirector()->end();

#if (CC_TARGET_PLATFORM == CC_PLATFORM_IOS)
    exit(0);
#endif
}

void HelloWorld::update(float dt){
    CCSize visibleSize = CCDirector::sharedDirector()->getVisibleSize();
    CCPoint origin = CCDirector::sharedDirector()->getVisibleOrigin();

    //移動
    for( std::list<Sprite>::iterator it = mSprites.begin() ; it!=mSprites.end(); ++it){
		it->update(dt);
	}

    //衝突・消滅判定
	for( std::list<Sprite>::iterator it = mSprites.begin() ; it!=mSprites.end();){
		if (&(*it) != mPlayer ) {
			if (it->isCollide(*mPlayer)) {
			    CCDirector::sharedDirector()->end();
			}
			if(it->mY < origin.y) {
				std::list<Sprite>::iterator j = it;
				++it;
				j->mSprite->getParent()->removeChild(j->mSprite, true);
				j->mSprite = 0;
				mSprites.erase(j);
			}
			else {
				++it;
			}
		}
		else{
			++it;
		}
	}

	//ブロック出現
	if (rand()%100 < 2) {

	    mSprites.push_back(Sprite());
	    Sprite* s = &mSprites.back();
	    s->mX = rand()%static_cast<int>(visibleSize.width) + origin.x;
	    s->mY = visibleSize.height + origin.y;
	    s->mVelocityY = -100.f - rand()%(mWave+1) * 6.f;
	    s->mSprite = CCSprite::create("blocks.png",CCRect(64,0,32,32));

	    float scale = 2.f + 0.2f * static_cast<float>(rand()%(mWave+1));
	    s->mSprite->setScale(scale);
	    s->mSize = 32.f * scale;
	    this->addChild(s->mSprite, 0);
	    s->mSprite->setZOrder(1);
	    s->mResist = 0.f;
	}

	// wave増加
	mTimeCount += dt;
	if (mTimeCount > 10.f) {
		mWave++;
		mTimeCount -= 10.f;

		std::stringstream ss;
		ss << "wave:";
		ss << mWave;
		mLabel->setString(ss.str().c_str());
	}
}


void HelloWorld::didAccelerate(CCAcceleration* pAccel)
{
	mPlayer->mAccelX = pAccel->x * 2048.f;
	mPlayer->mAccelY = pAccel->y * 2048.f;
}


HelloWorld::Sprite::Sprite(){
	mX = 0.f;
	mY = 0.f;
	mVelocityX = 0.f;
	mVelocityY = 0.f;
	mAccelX = 0.f;
	mAccelY = 0.f;
	mSprite =0;
	mSize = 0.f;
	mResist = 0.f;
}

void HelloWorld::Sprite::update(float dt){
	mX += mVelocityX * dt;
	mY += mVelocityY * dt;

	double res = pow(0.1, dt);
	mVelocityX *= (1.f - mResist) + res * mResist;
	mVelocityY *= (1.f - mResist) + res * mResist;

	mVelocityX += mAccelX * dt;
	mVelocityY += mAccelY * dt;

	mSprite->setPosition(ccp(mX, mY));
}

bool HelloWorld::Sprite::isCollide( const HelloWorld::Sprite& tar) const {
	if (this == &tar){
		return true;
	}

	float dx = mX - tar.mX;
	float dy = mY - tar.mY;
	float s = (mSize + tar.mSize) / 2.f;

	return (dx * dx < s * s) && (dy * dy < s * s);
}
