#ifndef __HELLOWORLD_SCENE_H__
#define __HELLOWORLD_SCENE_H__

#include "cocos2d.h"
#include "Sprite.h"

class HelloWorld : public cocos2d::CCLayer
{
	cocos2d::CCLabelTTF* mHighScoreLabel;
	cocos2d::CCLabelTTF* mLengthLabel;
	cocos2d::CCLabelTTF* mVelocityLabel;
	Sprite* mPlayer;
	std::list<Sprite*> mEnemies;
	std::list<Sprite*> mHoles;
	std::list<Sprite> mSprites;

	bool mStarted;
	bool mEnded;
	cocos2d::CCPoint mTouchStartPoint;
	float mGroundY;
	float mCameraX;
	float mLength;
	float mHighScore;
	cocos2d::CCSprite* mGround;
	cocos2d::CCSprite* mSky;

	std::vector<cocos2d::CCSprite*> mGuids;

	cocos2d::CCMenu* mHorizontalBoost;
	cocos2d::CCMenu* mUpBoost;
	cocos2d::CCMenu* mDownBoost;
	cocos2d::CCMenu* mBomb;
	cocos2d::CCMenu* mRetry;

	static const int MaxVelocity = 15 * 60;
	float mChargeBombCount;
	float mChargeUpCount;
	float mChargeHorizontalCount;
	float mChargeDownCount;
	static const int ChargeDelay = 10;

	float mNextEnemyPosition;
public:
    // Here's a difference. Method 'init' in cocos2d-x returns bool, instead of returning 'id' in cocos2d-iphone
    virtual bool init();  

    // there's no 'id' in cpp, so we recommend returning the class instance pointer
    static cocos2d::CCScene* scene();

    void onEnter();
    // a selector callback
    void menuCloseCallback(CCObject* pSender);
    bool ccTouchBegan(cocos2d::CCTouch *ptouch, cocos2d::CCEvent *pEvent);
    void ccTouchMoved(cocos2d::CCTouch *ptouch, cocos2d::CCEvent *pEvent);
    void ccTouchEnded(cocos2d::CCTouch *ptouch, cocos2d::CCEvent *pEvent);
    void ccTouchCancelled(cocos2d::CCTouch *ptouch, cocos2d::CCEvent *pEvent);

    void update(float dt);
    
    void pushButtonBomb();
    void pushButtonDownBoost();
    void pushButtonUpBoost();
    void pushButtonHorizontalBoost();

    // implement the "static node()" method manually
    CREATE_FUNC(HelloWorld);
private:
    void createEnemy(float x, bool air);
    void createHole(float x);
    cocos2d::CCPoint getAngle(cocos2d::CCTouch*);

    void createButton();
    void createEnemy();

    void refreshButtons();
    void createCutIn(const char*);

    void initGame();
};

#endif // __HELLOWORLD_SCENE_H__
