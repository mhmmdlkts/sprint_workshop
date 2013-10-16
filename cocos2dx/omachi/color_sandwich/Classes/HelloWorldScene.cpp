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
    block_size_ = visibleSize.width / kHorizontalNumber;
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
    
    CCLabelTTF* pLabel = CCLabelTTF::create("Hello World", "Arial", 24);

    // position the label on the center of the screen
    pLabel->setPosition(ccp(origin.x + visibleSize.width/2,
                            origin.y + visibleSize.height - pLabel->getContentSize().height));

    // add the label as a child to this layer
    this->addChild(pLabel, 1);

    // add "HelloWorld" splash screen"
    CCSprite* pSprite = CCSprite::create("HelloWorld.png");

    // position the sprite on the center of the screen
    pSprite->setPosition(ccp(visibleSize.width/2 + origin.x, visibleSize.height/2 + origin.y));

    // add the sprite as a child to this layer
    this->addChild(pSprite, 0);

    //ブロック初期化
    blocks_.resize(kHorizontalNumber * kVerticalNumber);
    for ( int i=0; i<kHorizontalNumber ; ++i) {
        for ( int j=0; j<kVerticalNumber ; ++j) {
        	CCSprite* sprite = CCSprite::create("blocks.png");
            this->addChild(sprite, 0);
        	block(i,j).init( block_size_,i,j,sprite );
        	block(i,j).setStateRandom();
        }
    }

   // this->scheduleUpdate();
	this->schedule( schedule_selector(HelloWorld::updateGame), 20.0 / 60.0 );

	CCDirector::sharedDirector()->getTouchDispatcher()->addTargetedDelegate(this, 0, true);
    return true;
}

void HelloWorld::update(float d){
    CCSize visibleSize = CCDirector::sharedDirector()->getVisibleSize();
    CCPoint origin = CCDirector::sharedDirector()->getVisibleOrigin();

    // add "HelloWorld" splash screen"
    CCSprite* pSprite = CCSprite::create("HelloWorld.png");

    // position the sprite on the center of the screen
    pSprite->setPosition(ccp(rand()%100,rand()%100));
    pSprite->setPosition(ccp(visibleSize.width/2 + origin.x + rand()%100, visibleSize.height/2 + origin.y + rand()%100));

    // add the sprite as a child to this layer
    this->addChild(pSprite, 0);
}

void HelloWorld::updateGame(float d){

	//連鎖処理

	bool requestFinish = false;
	//まず穴があいているところを移動させる
	for(int i=0 ; i<kHorizontalNumber ; ++i){
		for(int j=0 ; j<kVerticalNumber ; ++j){
			if (block(i,j).getState() == BLOCKSTATE_EMPTY ){
				if (j<kVerticalNumber-1) {
					block(i,j).setState(block(i,j + 1).getState());
					block(i,j + 1).setState(BLOCKSTATE_EMPTY);
				}
				else {
					block(i,j).setStateRandom();
				}
				requestFinish = true;
			}
		}
	}
	if (requestFinish){
		return;
	}
	//はさまってたら消す
	if (!remove_blocks_.empty()){
		for ( std::set<Block*>::const_iterator it=remove_blocks_.begin() ; it!=remove_blocks_.end(); ++it) {
			(*it)->setState(BLOCKSTATE_EMPTY);
		}
		remove_blocks_.clear();
		return;
	}

	const int minNumber = 0;
	for(int i=0 ; i<kHorizontalNumber ; ++i){
		for(int j=0 ; j<kVerticalNumber ; ++j){
			if (block(i,j).getState() != BLOCKSTATE_NEUTRAL ){
				for ( int k=1 ; true ; ++k) {
					if (i-k < 0){
						break;
					}
					if(block(i-k,j).getState() == BLOCKSTATE_NEUTRAL){
					}
					else if(block(i-k,j).getState() == block(i,j).getState()){
						//消す処理
						if (minNumber < k) {
							for( int m = 0 ; m<=k ; ++m){
								remove_blocks_.insert(&block(i-m,j));
							}
						}
						break;
					}
					else{
						break;
					}
				}
				for ( int k=1 ; true ; ++k) {
					if (j-k < 0){
						break;
					}
					if(block(i,j-k).getState() == BLOCKSTATE_NEUTRAL){
					}
					else if(block(i,j-k).getState() == block(i,j).getState()){
						//消す処理
						if (minNumber < k) {
							for( int m = 0 ; m<=k ; ++m){
								remove_blocks_.insert(&block(i,j-m));
							}
						}
						break;
					}
					else{
						break;
					}
				}
			}
		}
	}

	if (!remove_blocks_.empty()){
		//消えるブロックが存在する
		for ( std::set<Block*>::const_iterator it=remove_blocks_.begin() ; it!=remove_blocks_.end(); ++it) {
			(*it)->flash();
		}
	}
	else {
		CCDirector::sharedDirector()->getTouchDispatcher()->addTargetedDelegate(this, 0, true);
	}
}


bool HelloWorld::ccTouchBegan(cocos2d::CCTouch *ptouch, cocos2d::CCEvent *pEvent) {
    //タッチ開始p

	//範囲内かチェックする
	const unsigned int i = ptouch->getLocation().x / block_size_;
	const unsigned int j = ptouch->getLocation().y / block_size_;
	if( i < kHorizontalNumber && j < kVerticalNumber ){
		//タップした部分のブロックをけす
		block(i,j).setState(BLOCKSTATE_EMPTY);
		CCDirector::sharedDirector()->getTouchDispatcher()->removeDelegate(this);
	}

	return true;
}

void HelloWorld::ccTouchMoved(cocos2d::CCTouch *ptouch, cocos2d::CCEvent *pEvent) {
    //タッチ中
}

void HelloWorld::ccTouchCancelled(cocos2d::CCTouch *ptouch, cocos2d::CCEvent *pEvent) {
    //タッチキャンセル
}
void HelloWorld::ccTouchEnded(cocos2d::CCTouch *ptouch, cocos2d::CCEvent *pEvent) {
    CCSize visibleSize = CCDirector::sharedDirector()->getVisibleSize();
    CCPoint origin = CCDirector::sharedDirector()->getVisibleOrigin();
    {
		// add "HelloWorld" splash screen"
		CCSprite* pSprite = CCSprite::create("HelloWorld.png",CCRect(0.f,0.f,10.f,10.f));

		// position the sprite on the center of the screen
		pSprite->setPosition(ccp(
				ptouch->getLocation().x,
				ptouch->getLocation().y));

		// add the sprite as a child to this layer
		this->addChild(pSprite, 0);
    }
    {
		// add "HelloWorld" splash screen"
		CCSprite* pSprite = CCSprite::create("HelloWorld.png",CCRect(0.f,0.f,10.f,10.f));

		// position the sprite on the center of the screen
		pSprite->setPosition(ccp(
				ptouch->getStartLocation().x,
				ptouch->getStartLocation().y));

		// add the sprite as a child to this layer
		this->addChild(pSprite, 0);
    }
}

void HelloWorld::menuCloseCallback(CCObject* pSender)
{
    CCDirector::sharedDirector()->end();

#if (CC_TARGET_PLATFORM == CC_PLATFORM_IOS)
    exit(0);
#endif
}


HelloWorld::Block& HelloWorld::block(int x,int y){
	return blocks_.at(x * kVerticalNumber + y);
}

const HelloWorld::Block& HelloWorld::block(int x,int y) const{
	return blocks_.at(x * kVerticalNumber + y);
}

void HelloWorld::Block::setState(HelloWorld::BlockState state) {
	int s = 32;
	//色替え
	sprite_->setTextureRect(CCRect(s * state,0,s,s));
	sprite_->setScale(0.9f * static_cast<float>(size_) / s);

	if (state_ != state) {
		state_ = state;

		if (state != BLOCKSTATE_EMPTY) {
			sprite_->setPosition(position(x_,y_ + 1,size_));
			CCFiniteTimeAction* move = CCMoveTo::create( 5.f / 60.f, position(x_, y_, size_));
			sprite_->runAction(move);
		}
		else{
			sprite_->setPosition(position(x_,y_,size_));
		}
	}
}

void HelloWorld::Block::init(int blockSize, int x,int y,cocos2d::CCSprite* sprite) {
	size_ = blockSize;
	sprite_ = sprite;
	x_ = x;
	y_ = y;

	sprite_->setPosition( position(x,y,size_ ));
	state_ = BLOCKSTATE_EMPTY;
}

void HelloWorld::Block::setStateRandom(){
	if (rand()%3 == 0){
		setState(BLOCKSTATE_NEUTRAL);
	}
	else{
		setState(BlockState(rand()%6 + BLOCKSTATE_RED));
	}
}

cocos2d::CCPoint HelloWorld::Block::position(int x,int y,int size){
	return cocos2d::CCPoint(
			x * size + size/2,
			y * size + size/2);
}

void HelloWorld::Block::flash(){
	sprite_->setScale(static_cast<float>(size_) / 32 / 2);
}

