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

    //こっからゲーム部分
    this->scheduleUpdate();
	this->setTouchEnabled(true);

    this->addChild(player_.sprite_);

	CCDirector::sharedDirector()->getTouchDispatcher()->addTargetedDelegate(this, 0, true);

	height_ = 0.f;
	block_created_height_ = 0.f;
	return true;
}

void HelloWorld::onEnter() {
    CCLayer::onEnter();

        setAccelerometerEnabled(true);
    setAccelerometerInterval(1.0f / 60.0f);
	//デリゲートの設定
	//CCDirector::sharedDirector()->getTouchDispatcher()->addTargetedDelegate(this, 0, true);
}

//void HelloWorld::onExit() {
    //デリゲートの解除
	//CCDirector::sharedDirector()->getTouchDispatcher()->removeDelegate(this);
//}

void HelloWorld::update(float dt){
	CCSize visibleSize = CCDirector::sharedDirector()->getVisibleSize();

	//player
	player_.update(dt);

	//更新
	player_.sprite_->setPositionX(player_.position_x_);
    player_.sprite_->setPositionY(player_.position_y_ - height_);

    for(std::list<Block>::iterator it = blocks_.begin() ; it != blocks_.end() ; ++it){
		it->sprite_->setPositionX(it->x_);
		it->sprite_->setPositionY(it->y_ - height_);
    }

    //足場に着地したばあい
    if (player_.velocity_y_ < 0.f) {
		for(std::list<Block>::iterator it = blocks_.begin() ; it != blocks_.end() ; ++it){
			float x = player_.position_x_ - it->x_;
			float y = player_.position_y_ - it->y_;

			if (x * x + y * y < 100.f * 100.f) {
				player_.jump();
				break;
			}
		}
    }

    if (player_.position_y_ > height_ + visibleSize.height/2.f) {
    	height_ = player_.position_y_ - visibleSize.height/2.f;
    }

    //足場作成処理
    if (block_created_height_ <= height_ + 1000.f) {
    	block_created_height_ += 300.f;

    	blocks_.push_back(Block());
    	blocks_.back().sprite_ = CCSprite::create("blocks.png",CCRect(32,0,32,32));
    	blocks_.back().sprite_->setScale(3.f);
        this->addChild(blocks_.back().sprite_);

    	blocks_.back().x_ = rand() % static_cast<int>(visibleSize.width);
    	blocks_.back().y_ = block_created_height_;
    }

    if( player_.position_y_ < height_ - 400.f) {
    	 CCDirector::sharedDirector()->end();
    }
}

bool HelloWorld::ccTouchBegan(cocos2d::CCTouch *ptouch, cocos2d::CCEvent *pEvent) {
 //   CCDirector::sharedDirector()->end();
	return true;
}

void HelloWorld::ccTouchMoved(cocos2d::CCTouch *ptouch, cocos2d::CCEvent *pEvent) {
  //  CCDirector::sharedDirector()->end();

    //タッチ中
	CCPoint location = ptouch->getLocation();
	if (location.x > player_.position_x_) {
		player_.velocity_x_ = 400.f;
	}
	else {
		player_.velocity_x_ = -400.f;
	}
}

void HelloWorld::ccTouchCancelled(cocos2d::CCTouch *ptouch, cocos2d::CCEvent *pEvent) {
    //タッチキャンセル

}
void HelloWorld::ccTouchEnded(cocos2d::CCTouch *ptouch, cocos2d::CCEvent *pEvent) {
    CCSize visibleSize = CCDirector::sharedDirector()->getVisibleSize();
    CCPoint origin = CCDirector::sharedDirector()->getVisibleOrigin();

    player_.velocity_x_ = 0.f;
}

void HelloWorld::menuCloseCallback(CCObject* pSender)
{
    CCDirector::sharedDirector()->end();

#if (CC_TARGET_PLATFORM == CC_PLATFORM_IOS)
    exit(0);
#endif
}

void HelloWorld::registerWithTouchDispatcher()
{
	// CCTouchDispatcher::sharedDispatcher()->addTargetedDelegate(this,0,true);
    CCDirector::sharedDirector()->getTouchDispatcher()->addStandardDelegate(this,0);
}

void HelloWorld::didAccelerate(CCAcceleration* pAccel)
{
	player_.velocity_x_ -= pAccel->x;
    //ax = pAccel->x;
    //ay = pAccel->y;
    //az = pAccel->z;
}

HelloWorld::Player::Player(){
   CCSize visibleSize = CCDirector::sharedDirector()->getVisibleSize();
   CCPoint origin = CCDirector::sharedDirector()->getVisibleOrigin();

   sprite_ = CCSprite::create("blocks.png",CCRect(64,0,32,32));
   sprite_->setScale(1.f);
   sprite_->setZOrder(1);
   position_x_ = visibleSize.width / 2;
   position_y_ = 0.f;
   velocity_x_ = 0.f;
   velocity_y_ = 2.f;
   gravity_ = -9.8f * 400.f;
}

void HelloWorld::Player::update(float dt){
	position_x_ += velocity_x_ * dt;
	position_y_ += velocity_y_ * dt + gravity_ * dt * dt / 2.f;

	velocity_y_ += gravity_ * dt;

	if (position_y_ < 0.f){
		jump();
	}
}

void HelloWorld::Player::jump(){
	velocity_y_ = -gravity_ / 2.f;
}

