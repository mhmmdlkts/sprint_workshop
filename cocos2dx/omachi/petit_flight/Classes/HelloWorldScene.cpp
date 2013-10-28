#include "HelloWorldScene.h"

#include <iostream>
#include <sstream>
#include "sprite.h"

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
    
    mHighScoreLabel = CCLabelTTF::create("HighScore", "Arial", 64);
    this->addChild(mHighScoreLabel, 1);
    mHighScoreLabel->setPosition(ccp(origin.x + visibleSize.width - 200.f,
                            origin.y + visibleSize.height - mHighScoreLabel->getContentSize().height));


    mLengthLabel = CCLabelTTF::create("距離", "Arial", 64);
    this->addChild(mLengthLabel, 1);
    mLengthLabel->setPosition(ccp(origin.x + visibleSize.width - 200.f,
                            origin.y + visibleSize.height - mLengthLabel->getContentSize().height * 2));

    mVelocityLabel = CCLabelTTF::create("速度", "Arial", 64);
    this->addChild(mVelocityLabel, 1);
    mVelocityLabel->setPosition(ccp(origin.x + visibleSize.width - 200.f,
                            origin.y + visibleSize.height - mVelocityLabel->getContentSize().height * 3));

    this->scheduleUpdate();
    //this->schedule(schedule_selector(HelloWorld::update), 1.0f / 60.f);
    CCDirector::sharedDirector()->getTouchDispatcher()->addTargetedDelegate(this, 0, true);

    mHighScore = 0.f;
    mPlayer = 0;
    mGround = 0;
    mSky = 0;
	mHorizontalBoost = 0;
	mUpBoost = 0;
	mDownBoost = 0;
	mBomb = 0;
	mRetry = 0;

    initGame();
	return true;
}

void HelloWorld::initGame()
{
    mStarted = false;
    mEnded = false;
	mCameraX = -400.f;
	mGroundY = 200.f + 75.f;
	mLength = 0.f;

	mChargeUpCount = 0.f;
	mChargeDownCount = 0.f;
	mChargeHorizontalCount = 0.f;
	mChargeBombCount = 0.f;

	//消す
	for(std::list<Sprite>::iterator it=mSprites.begin();it!=mSprites.end() ; ++it){
		it->mSprite->getParent()->removeChild(it->mSprite,true);
	}
	mSprites.clear();
	mEnemies.clear();
	mHoles.clear();

	//プレイヤーキャラを配置
    mSprites.push_back(Sprite());
    mPlayer = &mSprites.back();
    mPlayer->mSprite = CCSprite::create("player.png");
    mPlayer->mSprite->setScale(0.5f);
    this->addChild(mPlayer->mSprite, 0);
    this->reorderChild(mPlayer->mSprite,5);
    mPlayer->mSize = 16.f;
    mPlayer->mY = mGroundY + mPlayer->mSize / 2.f;

    if (mGround == 0) {
		mGround = CCSprite::create("ground.png");
		mGround->setScale(2.f);
		this->addChild(mGround, -1);
    }
    if (mSky == 0) {
		mSky = CCSprite::create("sky.png");
		mSky->setScale(2.f);
		this->addChild(mSky, -2);
    }

    /////////////////////////////
    //アビリティボタン
	refreshButtons();

	if (mRetry){
		this->removeChild(mRetry, true);
		mRetry = 0;
	}

    mNextEnemyPosition = 600.f;
}

void HelloWorld::onEnter() {
    CCLayer::onEnter();
}

void HelloWorld::menuCloseCallback(CCObject* pSender)
{
    CCDirector::sharedDirector()->end();

#if (CC_TARGET_PLATFORM == CC_PLATFORM_IOS)
    exit(0);
#endif
}

bool HelloWorld::ccTouchBegan(cocos2d::CCTouch *ptouch, cocos2d::CCEvent *pEvent) {
	mTouchStartPoint = this->convertTouchToNodeSpace(ptouch);

	if (!mStarted){
		mGuids.resize(8);
		for (std::vector<CCSprite*>::iterator it=mGuids.begin(); it!=mGuids.end() ; ++it){
			*it = CCSprite::create("blocks.png",CCRect(64.f,0.f,32.f,32.f));
			(*it)->setPosition(ccp(mPlayer->mX - mCameraX,mPlayer->mY));
		    this->reorderChild(*it,9);
			this->addChild(*it, 1);
		}
	}
	return true;
}

void HelloWorld::ccTouchMoved(cocos2d::CCTouch *ptouch, cocos2d::CCEvent *pEvent) {
  //  CCDirector::sharedDirector()->end();
    CCSize visibleSize = CCDirector::sharedDirector()->getVisibleSize();
    CCPoint origin = CCDirector::sharedDirector()->getVisibleOrigin();

	if (!mStarted) {
        CCSize visibleSize = CCDirector::sharedDirector()->getVisibleSize();
        CCPoint origin = CCDirector::sharedDirector()->getVisibleOrigin();
    	CCPoint ang = getAngle(ptouch);

    	float length = 256.f;
    	int index = 0;
    	for (std::vector<CCSprite*>::iterator it=mGuids.begin(); it!=mGuids.end() ; ++it){
    		float dx = ang.x * length * index / mGuids.size();
    		float dy = ang.y * length * index / mGuids.size();
			(*it)->setPosition(ccp(mPlayer->mX - mCameraX + dx,mPlayer->mY + dy));
			++index;
		}

	}
}

void HelloWorld::ccTouchCancelled(cocos2d::CCTouch *ptouch, cocos2d::CCEvent *pEvent) {
    //タッチキャンセル
	if (!mStarted) {
		for (std::vector<CCSprite*>::iterator it=mGuids.begin(); it!=mGuids.end() ; ++it){
			(*it)->getParent()->removeChild(*it,true);
		}
		mGuids.clear();
	}
}

cocos2d::CCPoint HelloWorld::getAngle(cocos2d::CCTouch* ptouch) {
    CCSize visibleSize = CCDirector::sharedDirector()->getVisibleSize();
    CCPoint origin = CCDirector::sharedDirector()->getVisibleOrigin();
	CCPoint touchEndPoint = this->convertTouchToNodeSpace(ptouch);

//	CCPoint touchEndPoint(ptouch->getLocation().x,
//	ptouch->getLocation().y);

	float x = touchEndPoint.x - mTouchStartPoint.x;
	float y = touchEndPoint.y - mTouchStartPoint.y;
	float d = sqrtf(x * x + y * y);
	if (d>0.f) {
		x /= d;
		y /= d;
	}
	return ccp(x,y);
}

void HelloWorld::ccTouchEnded(cocos2d::CCTouch *ptouch, cocos2d::CCEvent *pEvent) {

    //プレイヤーの方向をきめる
    //パチンコ式なので入力とは逆方向にとばす
    if ( !mStarted ) {
        CCSize visibleSize = CCDirector::sharedDirector()->getVisibleSize();
        CCPoint origin = CCDirector::sharedDirector()->getVisibleOrigin();
    	CCPoint ang = getAngle(ptouch);

    	//発射速度
    	float v = MaxVelocity * 1.5;

    	if (-ang.x > 0.f) {
			mStarted = true;
			mPlayer->mVelocityX = -ang.x * v;
			mPlayer->mVelocityY = -ang.y * v;
			mPlayer->mAccelY = -9.f * 60.f;

			mPlayer->mRotationVelocity = -ang.y * 60.f;
    	}

		for (std::vector<CCSprite*>::iterator it=mGuids.begin(); it!=mGuids.end() ; ++it){
			(*it)->getParent()->removeChild(*it,true);
			*it = 0;
		}
		mGuids.clear();
    }
}

void HelloWorld::update(float dt) {
    CCSize visibleSize = CCDirector::sharedDirector()->getVisibleSize();

    mChargeBombCount -= dt;
    mChargeUpCount -= dt;
    mChargeDownCount -= dt;
    mChargeHorizontalCount -= dt;

    if ((mChargeBombCount < 0.f && mChargeBombCount > -dt) ||
		(mChargeUpCount < 0.f && mChargeUpCount > -dt) ||
		(mChargeDownCount < 0.f && mChargeDownCount > -dt) ||
		(mChargeHorizontalCount < 0.f && mChargeHorizontalCount > -dt)){
    	refreshButtons();
    }

    //プレイヤー
    //バウンド
	if (mPlayer->mVelocityY < 0.f && mPlayer->mY < mGroundY) {
		mPlayer->mVelocityY *= -0.95f;
		mPlayer->mVelocityX *= 0.95f;

		//落とし穴当たり判定
	    for( std::list<Sprite*>::iterator it = mHoles.begin() ; it!=mHoles.end();){
	    	if ((*it)->mX < mCameraX - 2000.f) {
	    		// 通過したら消してしまえ
	    		(*it) -> mDestroyed = true;
				std::list<Sprite*>::iterator j = it;
	    		++it;
	    		mHoles.erase(j);
	    	}
	    	else{
		    	float x = mPlayer->mX - (*it)->mX;
		    	if (x*x < ((*it)->mSize * (*it)->mSize / 2.f)) {
		    		mEnded = true;
		    	}
		    	++it;
	    	}
	    }
	}
	if (mStarted) {
		if (mCameraX < mPlayer->mX - 200.f) {
			mCameraX = (mCameraX * .9f) + (mPlayer->mX - 200.f) * .1f;
		}
	}
	//エネミー当たり判定
    for( std::list<Sprite*>::iterator it = mEnemies.begin() ; it!=mEnemies.end();){
    	if ((*it)->mX < mCameraX - 2000.f) {
    		// 通過したら消してしまえ
    		(*it) -> mDestroyed = true;
			std::list<Sprite*>::iterator j = it;
    		++it;
    		mEnemies.erase(j);
    	}
    	else{
			if ((*it)->isCollide(*mPlayer)) {
				mEnded = true;
			}

			//移動する敵の処理
			if ((*it)->mInfo==1){
				(*it)->mX += cos((*it)->mCount * 2.f) * dt * 300.f;
			}
			if ((*it)->mInfo==2){
				(*it)->mY += cos((*it)->mCount * 2.f) * dt * 200.f;
			}

			++it;
    	}
    }

    createEnemy();
	//ブロック出現

/*
    if (mStarted && !mEnded) {
		if (rand()%100 < 2) {
			mSprites.push_back(Sprite());
			Sprite* s = &mSprites.back();
			s->mX = mCameraX + visibleSize.width;
			s->mY = rand()%static_cast<int>(visibleSize.height);
			s->mSprite = CCSprite::create("blocks.png",CCRect(64,0,32,32));

			float scale = 2.f;
			s->mSprite->setScale(scale);
			s->mSize = 32.f;
			this->addChild(s->mSprite, 0);
			s->mSprite->setZOrder(1);
			s->mResist = 0.f;
		}
	}
*/
	//背景
	float gx = fmod(mCameraX, 512.f);
	mGround->setPosition(ccp(-gx + 800.f, -512.f * 2.f + mGroundY));
	float sx = fmod(mCameraX/2.f, 512.f);
	mSky->setPosition(ccp(-sx + 800.f ,0.f));

    //移動
	if (!mEnded) {
		for( std::list<Sprite>::iterator it = mSprites.begin() ; it!=mSprites.end();){
			if (it->mDestroyed){
				std::list<Sprite>::iterator j = it;
	    		++it;
				j->mSprite->getParent()->removeChild(j->mSprite,true);
	    		mSprites.erase(j);
			}
			else {
				it->update(dt, mCameraX);
				++it;
			}

		}
	}

    mLength = mPlayer->mX;
    if (mHighScore < mLength) {
    	mHighScore = mLength;
    }

    {
		std::stringstream ss;
		const int t = mHighScore;
		ss << "HighScore ";
		ss << t / 100;
		ss << ".";
		ss << t % 100;
		ss << "m";
		mHighScoreLabel->setString(ss.str().c_str());
    }
    {
		std::stringstream ss;
		const int t = mLength;
		ss << t / 100;
		ss << ".";
		ss << t % 100;
		ss << "m";
		mLengthLabel->setString(ss.str().c_str());
    }
    {
		std::stringstream ss;
		const int t = mPlayer->mVelocityX;
		ss << t / 100;
		ss << ".";
		ss << t % 100;
		ss << "m/s";
		mVelocityLabel->setString(ss.str().c_str());
    }

    if (mEnded) {
    	if (mRetry == 0){
    	    CCPoint origin = CCDirector::sharedDirector()->getVisibleOrigin();
			cocos2d::CCMenuItemImage* img = CCMenuItemImage::create("retry.png","retry.png",this,menu_selector(HelloWorld::initGame));
			img->setPosition(ccp(origin.x + visibleSize.width / 2.f,origin.y + visibleSize.height / 2.f));

			mRetry = CCMenu::create(img, NULL);
			mRetry->setPosition(CCPointZero);
			this->addChild(mRetry, 2);
    	}
    }
}
void HelloWorld::createCutIn(const char* file){
    CCSize size = CCDirector::sharedDirector()->getVisibleSize();

    CCSprite* pSprite = CCSprite::create(file);
    pSprite->setPosition(ccp(0.f,size.height / 2.f));
	this->addChild(pSprite, -1);
	pSprite->runAction(
			CCSequence::create(
				CCMoveTo::create(0.3f, ccp(size.width / 2.f + 400.f,size.height / 2.f)),
				CCDelayTime::create(0.3f),
				CCRemoveSelf::create(true),
				NULL
			)
	);
}

/////////////////////////////////////////////////////////////////////////

void HelloWorld::pushButtonHorizontalBoost(){
	if (mStarted && !mEnded){
		if (mChargeHorizontalCount <= 0.f) {
			mPlayer->mVelocityX = static_cast<float>(MaxVelocity);
			mPlayer->mVelocityY = 0.f;
			mChargeHorizontalCount = ChargeDelay;
	    	refreshButtons();
	    	createCutIn("horizontal.jpg");
		}
	}
}
void HelloWorld::pushButtonUpBoost(){
	if (mStarted && !mEnded){
		if (mChargeUpCount <= 0.f) {
			mPlayer->mVelocityX = static_cast<float>(MaxVelocity) / sqrt(2.f);
			mPlayer->mVelocityY = static_cast<float>(MaxVelocity) / sqrt(2.f);
			mChargeUpCount = ChargeDelay;
	    	refreshButtons();
	    	createCutIn("up.jpg");
		}
	}
}
void HelloWorld::pushButtonDownBoost(){
	if (mStarted && !mEnded){
		if (mChargeDownCount <= 0.f) {
			mPlayer->mVelocityX = static_cast<float>(MaxVelocity) / sqrt(2.f);
			mPlayer->mVelocityY = -static_cast<float>(MaxVelocity) / sqrt(2.f);
			mChargeDownCount = ChargeDelay;
	    	refreshButtons();
	    	createCutIn("down.jpg");
		}
	}
}
void HelloWorld::pushButtonBomb(){
	if (mStarted && !mEnded){
		if (mChargeBombCount <= 0.f) {
			//近くにいる敵を消す
		    for( std::list<Sprite*>::iterator it = mEnemies.begin() ; it!=mEnemies.end();){
		    	float d = (*it)->mX - mPlayer->mX;
		    	if (d * d < 4000.f * 4000.f ) {
		    		(*it) -> mDestroyed = true;
					std::list<Sprite*>::iterator j = it;
		    		++it;
		    		mEnemies.erase(j);
		    	}
		    	else{
			    	++it;
		    	}
		    }

			mChargeBombCount = ChargeDelay;
	    	refreshButtons();
	    	createCutIn("attack.jpg");
		}
	}
}

void HelloWorld::refreshButtons() {
    CCPoint origin = CCDirector::sharedDirector()->getVisibleOrigin();
    CCSize visibleSize = CCDirector::sharedDirector()->getVisibleSize();

	if (mUpBoost){
		this->removeChild(mUpBoost, true);
		mUpBoost = 0;
	}
	if ( mChargeUpCount <= 0.f){
		cocos2d::CCMenuItemImage*mUpBoostOn = CCMenuItemImage::create("button2.png","button2.png",this,menu_selector(HelloWorld::pushButtonUpBoost));
		mUpBoostOn->setPosition(ccp(origin.x + 400.f,origin.y + mUpBoostOn->getContentSize().height/2*0.75f));
		mUpBoostOn->setScale(0.75f);

		mUpBoost = CCMenu::create(mUpBoostOn, NULL);
		mUpBoost->setPosition(CCPointZero);
		this->addChild(mUpBoost, 1);
	}
	else {
		cocos2d::CCMenuItemImage*mUpBoostOff = CCMenuItemImage::create("button2_.png","button2_.png",this,menu_selector(HelloWorld::pushButtonUpBoost));
		mUpBoostOff->setPosition(ccp(origin.x + 400.f,origin.y + mUpBoostOff->getContentSize().height/2*0.75f));
		mUpBoostOff->setScale(0.75f);

		mUpBoost = CCMenu::create(mUpBoostOff, NULL);
		mUpBoost->setPosition(CCPointZero);
		this->addChild(mUpBoost, 1);
	}

	if (mHorizontalBoost){
		this->removeChild(mHorizontalBoost, true);
		mHorizontalBoost = 0;
	}
	if ( mChargeHorizontalCount <= 0.f){
		cocos2d::CCMenuItemImage*mHorizontalBoostOn = CCMenuItemImage::create("button3.png","button3.png",this,menu_selector(HelloWorld::pushButtonHorizontalBoost));
		mHorizontalBoostOn->setPosition(ccp(origin.x + 400.f + 200,origin.y + mHorizontalBoostOn->getContentSize().height/2*0.75f));
		mHorizontalBoostOn->setScale(0.75f);
		mHorizontalBoost = CCMenu::create(mHorizontalBoostOn, NULL);
		mHorizontalBoost->setPosition(CCPointZero);
		this->addChild(mHorizontalBoost, 1);
	}
	else{
		cocos2d::CCMenuItemImage*mHorizontalBoostOff = CCMenuItemImage::create("button3_.png","button3_.png",this,menu_selector(HelloWorld::pushButtonHorizontalBoost));
		mHorizontalBoostOff->setPosition(ccp(origin.x + 400.f + 200,origin.y + mHorizontalBoostOff->getContentSize().height/2*0.75f));
		mHorizontalBoostOff->setScale(0.75f);
		mHorizontalBoost = CCMenu::create(mHorizontalBoostOff, NULL);
		mHorizontalBoost->setPosition(CCPointZero);
		this->addChild(mHorizontalBoost, 1);
	}

	if (mDownBoost){
		this->removeChild(mDownBoost, true);
		mDownBoost = 0;
	}
	if ( mChargeDownCount <= 0.f){
		cocos2d::CCMenuItemImage*mDownBoostOn = CCMenuItemImage::create("button4.png","button4.png",this,menu_selector(HelloWorld::pushButtonDownBoost));
		mDownBoostOn->setPosition(ccp(origin.x + 400.f + 400,origin.y + mDownBoostOn->getContentSize().height/2*0.75f));
		mDownBoostOn->setScale(0.75f);
		mDownBoost = CCMenu::create(mDownBoostOn, NULL);
		mDownBoost->setPosition(CCPointZero);
		this->addChild(mDownBoost, 1);
	}
	else{
		cocos2d::CCMenuItemImage*mDownBoostOff = CCMenuItemImage::create("button4_.png","button4_.png",this,menu_selector(HelloWorld::pushButtonDownBoost));
		mDownBoostOff->setPosition(ccp(origin.x + 400.f + 400,origin.y + mDownBoostOff->getContentSize().height/2*0.75f));
		mDownBoostOff->setScale(0.75f);
		mDownBoost = CCMenu::create(mDownBoostOff, NULL);
		mDownBoost->setPosition(CCPointZero);
		this->addChild(mDownBoost, 1);
	}

	if (mBomb){
		this->removeChild(mBomb, true);
		mBomb = 0;
	}
	if ( mChargeBombCount <= 0.f){
		cocos2d::CCMenuItemImage*mBombOn = CCMenuItemImage::create("button1.png","button1.png",this,menu_selector(HelloWorld::pushButtonBomb));
		mBombOn->setPosition(ccp(origin.x + 400.f + 600,origin.y + mBombOn->getContentSize().height/2*0.75f));
		mBombOn->setScale(0.75f);
		mBomb = CCMenu::create(mBombOn, NULL);
		mBomb->setPosition(CCPointZero);
		this->addChild(mBomb, 1);
	}
	else{
		cocos2d::CCMenuItemImage*mBombOff = CCMenuItemImage::create("button1_.png","button1_.png",this,menu_selector(HelloWorld::pushButtonBomb));
		mBombOff->setPosition(ccp(origin.x + 400.f + 600,origin.y + mBombOff->getContentSize().height/2*0.75f));
		mBombOff->setScale(0.75f);
		mBomb = CCMenu::create(mBombOff, NULL);
		mBomb->setPosition(CCPointZero);
		this->addChild(mBomb, 1);
	}


}

/////////////////////////////////////////////////////////////////////////
void HelloWorld::createEnemy(float x, bool air){
	float size = 64.f;
	mSprites.push_back(Sprite());
	Sprite* s = &mSprites.back();
	s->mX = x;
	s->mY = mGroundY + size / 2.f;
	if (air) {
		s->mY += 200.f;
		if (rand() % 5 == 0) {
			s->mSprite = CCSprite::create("enemy_air2.png");
			//横移動
			s->mInfo = 2;
		}
		else{
			s->mSprite = CCSprite::create("enemy_air.png");
		}
	}
	else{
		if (rand() % 5 == 0) {
			s->mSprite = CCSprite::create("enemy2.png");
			//縦移動
			s->mInfo = 1;
		}
		else{
			s->mSprite = CCSprite::create("enemy.png");
		}
	}
	s->mSize = size;
	this->addChild(s->mSprite, 0);
	s->mSprite->setZOrder(0);
	s->mSprite->setScale(0.5f);
	s->mResist = 0.f;


	mEnemies.push_back(s);
}

void HelloWorld::createHole(float x){
	float size = 64.f;
	mSprites.push_back(Sprite());
	Sprite* s = &mSprites.back();
	s->mX = x;
	s->mY = mGroundY - size / 2.f;
	s->mSprite = CCSprite::create("hole.png");
	s->mSize = size;
	this->addChild(s->mSprite, 0);
	s->mSprite->setZOrder(0);
	s->mResist = 0.f;

	mHoles.push_back(s);
}

void HelloWorld::createEnemy() {
	while (mNextEnemyPosition < mCameraX + 2000.f) {
		int r = rand() % 2;
		if ( r == 0)
			createEnemy(mNextEnemyPosition, rand() % 2);
		else if (r==1){
			createHole(mNextEnemyPosition);
		}
		mNextEnemyPosition += rand() % 1600 + 400;
	}
}
/////////////////////////////////////////////////////////////////////////

Sprite::Sprite(){
	mX = 0.f;
	mY = 0.f;
	mVelocityX = 0.f;
	mVelocityY = 0.f;
	mAccelX = 0.f;
	mAccelY = 0.f;
	mSprite =0;
	mSize = 0.f;
	mResist = 0.f;

	mRotation = 0.f;
	mRotationVelocity = 0.f;
	mCount = 0.f;

	mDestroyed = false;
	mInfo = 0;
}

void Sprite::update(float dt, float cameraX){
	mX += mVelocityX * dt;
	mY += mVelocityY * dt;

	double res = pow(0.1, dt);
	mVelocityX *= (1.f - mResist) + res * mResist;
	mVelocityY *= (1.f - mResist) + res * mResist;

	mVelocityX += mAccelX * dt;
	mVelocityY += mAccelY * dt;

	mRotation += mRotationVelocity *dt;

	mSprite->setPosition(ccp(mX - cameraX, mY));
	mSprite->setRotation(mRotation);

	mCount += dt;
}

bool Sprite::isCollide( const Sprite& tar) const {
	if (this == &tar){
		return true;
	}

	float dx = mX - tar.mX;
	float dy = mY - tar.mY;
	float s = (mSize + tar.mSize) / 2.f;

	return (dx * dx < s * s) && (dy * dy < s * s);
}

