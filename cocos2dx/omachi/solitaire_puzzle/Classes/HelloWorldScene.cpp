#include "HelloWorldScene.h"

#include <iostream>
#include <sstream>

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
    
    CCLabelTTF* pLabel = CCLabelTTF::create("", "Arial", 24);

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
            CCLabelTTF* label = CCLabelTTF::create("", "Arial", 100);
            this->addChild(label, 1);
        	block(i,j).init( block_size_,i,j,label );
        	block(i,j).setStateRandom();
        }
    }

    current_number_ =-1;
    label_ = CCLabelTTF::create("tap number", "Arial", 100);
    this->addChild(label_, 1);
    label_->setPosition(ccp(origin.x + visibleSize.width/2,
                            origin.y + visibleSize.height - label_->getContentSize().height));

    CCDirector::sharedDirector()->getTouchDispatcher()->addTargetedDelegate(this, 0, true);
    return true;
}

bool HelloWorld::ccTouchBegan(cocos2d::CCTouch *ptouch, cocos2d::CCEvent *pEvent) {
    //タッチ開始p

	//範囲内かチェックする
	const unsigned int i = ptouch->getLocation().x / block_size_;
	const unsigned int j = ptouch->getLocation().y / block_size_;
	if( i < kHorizontalNumber && j < kVerticalNumber ){
		//タップした部分のブロックをけす
		if (block(i,j).getEnable()) {
			int d = current_number_ - block(i,j).getState();
			const int x = i - last_i_;
			const int y = j - last_j_;
			if ( (current_number_ == -1 ) ||
				( (x * x) <= 1 && (y * y) <= 1 ) && ( (d == 1) || (d == -1) || (d == kMAX_NUMBER-1) || (d == 1-kMAX_NUMBER) ) ) {
				block(i,j).setEnable(false);
				current_number_ = block(i,j).getState();
				last_i_ = i;
				last_j_ = j;
			}
		}
	}
	else {
		current_number_ = rand()%kMAX_NUMBER + 1;
	}
	std::stringstream ss;
	ss << current_number_;
	label_->setString(ss.str().c_str());

	return true;
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

void HelloWorld::Block::setEnable(bool v) {
	enable_ = v;
	if (!enable_) {
		label_->setString("");
	}
}

void HelloWorld::Block::setState(int v) {
	value_ = v;

	std::stringstream ss;
	ss << v;
	label_->setString(ss.str().c_str());
}

void HelloWorld::Block::init(int blockSize, int x,int y,cocos2d::CCLabelTTF* label) {
	size_ = blockSize;
	label_ = label;
	x_ = x;
	y_ = y;

	label_->setPosition( position(x,y,size_ ));
	enable_ = true;
}

void HelloWorld::Block::setStateRandom(){
	setState(rand()%kMAX_NUMBER + 1);
}

cocos2d::CCPoint HelloWorld::Block::position(int x,int y,int size){
	return cocos2d::CCPoint(
			x * size + size/2,
			y * size + size/2);
}

