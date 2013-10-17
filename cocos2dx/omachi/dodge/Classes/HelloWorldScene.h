#ifndef __HELLOWORLD_SCENE_H__
#define __HELLOWORLD_SCENE_H__

#include "cocos2d.h"

class HelloWorld : public cocos2d::CCLayer
{
	class Sprite {
	public:
		cocos2d::CCSprite* mSprite;
	    float mX,mY;
		float mVelocityX,mVelocityY;
		float mAccelX,mAccelY;
		float mSize;
		float mResist;

		Sprite();
		void update(float dt);
		bool isCollide( const HelloWorld::Sprite& ) const;
	};

	Sprite* mPlayer;
	std::list< Sprite > mSprites;
	int mWave;
	float mTimeCount;
	cocos2d::CCLabelTTF* mLabel;
public:
    // Here's a difference. Method 'init' in cocos2d-x returns bool, instead of returning 'id' in cocos2d-iphone
    virtual bool init();  

    // there's no 'id' in cpp, so we recommend returning the class instance pointer
    static cocos2d::CCScene* scene();
    
    // a selector callback
    void menuCloseCallback(CCObject* pSender);
    
    // implement the "static node()" method manually
    CREATE_FUNC(HelloWorld);

    void didAccelerate(cocos2d::CCAcceleration* pAccel); //加速度取得
    void update(float dt);
};

#endif // __HELLOWORLD_SCENE_H__
