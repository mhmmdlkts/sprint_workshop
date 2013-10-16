#ifndef __HELLOWORLD_SCENE_H__
#define __HELLOWORLD_SCENE_H__

#include <vector>
#include "cocos2d.h"


class HelloWorld : public cocos2d::CCLayer
{
	class Player{
	public:
		float gravity_;
		float velocity_x_;
		float velocity_y_;
		float position_x_;
		float position_y_;
		cocos2d::CCSprite* sprite_;
		Player();
		void update(float dt);
		void jump();
	};

	class Block{
	public:
		float x_;
		float y_;
		cocos2d::CCSprite* sprite_;
	};

	Player player_;
	std::list<Block> blocks_;
	float height_;
	float block_created_height_;

public:
    // Here's a difference. Method 'init' in cocos2d-x returns bool, instead of returning 'id' in cocos2d-iphone
    virtual bool init();
    virtual void onEnter();

    // there's no 'id' in cpp, so we recommend returning the class instance pointer
    static cocos2d::CCScene* scene();
    
    // a selector callback
    void menuCloseCallback(CCObject* pSender);

	void registerWithTouchDispatcher();
    bool ccTouchBegan(cocos2d::CCTouch *ptouch, cocos2d::CCEvent *pEvent);
    void ccTouchMoved(cocos2d::CCTouch *ptouch, cocos2d::CCEvent *pEvent);
    void ccTouchEnded(cocos2d::CCTouch *ptouch, cocos2d::CCEvent *pEvent);
    void ccTouchCancelled(cocos2d::CCTouch *ptouch, cocos2d::CCEvent *pEvent);

    void didAccelerate(cocos2d::CCAcceleration* pAccel); //加速度取得

    // implement the "static node()" method manually
    CREATE_FUNC(HelloWorld);
private:
    Block& block(int x,int y);
    const Block& block(int x,int y) const;

protected:
    void update(float dt);
};

#endif // __HELLOWORLD_SCENE_H__
