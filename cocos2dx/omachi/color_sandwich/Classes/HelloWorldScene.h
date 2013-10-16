#ifndef __HELLOWORLD_SCENE_H__
#define __HELLOWORLD_SCENE_H__

#include <vector>
#include "cocos2d.h"


class HelloWorld : public cocos2d::CCLayer
{
	enum BlockState {
		BLOCKSTATE_EMPTY = 0,
		BLOCKSTATE_NEUTRAL = 1,
		BLOCKSTATE_RED = 2,
		BLOCKSTATE_GREEN = 3,
		BLOCKSTATE_BLUE = 4,
		BLOCKSTATE_C = 5,
		BLOCKSTATE_M = 6,
		BLOCKSTATE_Y = 7,
	};
	class Block{
		BlockState state_;
		cocos2d::CCSprite* sprite_;
		int x_;
		int y_;
		int size_;
	public:
		void init(int size,int x,int y,cocos2d::CCSprite*);
		void setState(BlockState);
		void setStateRandom();
		BlockState getState() const { return state_;}
		static cocos2d::CCPoint position(int x,int y,int size);
		void flash();
	};

	static const int kHorizontalNumber = 8;
	static const int kVerticalNumber = 8;
	std::vector< Block > blocks_;

	int block_size_;
	std::set<Block*> remove_blocks_;

public:
    // Here's a difference. Method 'init' in cocos2d-x returns bool, instead of returning 'id' in cocos2d-iphone
    virtual bool init();  

    // there's no 'id' in cpp, so we recommend returning the class instance pointer
    static cocos2d::CCScene* scene();
    
    // a selector callback
    void menuCloseCallback(CCObject* pSender);

//    void onEnter();
//    void onExit();
    bool ccTouchBegan(cocos2d::CCTouch *ptouch, cocos2d::CCEvent *pEvent);
    void ccTouchMoved(cocos2d::CCTouch *ptouch, cocos2d::CCEvent *pEvent);
    void ccTouchEnded(cocos2d::CCTouch *ptouch, cocos2d::CCEvent *pEvent);
    void ccTouchCancelled(cocos2d::CCTouch *ptouch, cocos2d::CCEvent *pEvent);

    // implement the "static node()" method manually
    CREATE_FUNC(HelloWorld);
private:
    Block& block(int x,int y);
    const Block& block(int x,int y) const;

protected:
    void update(float dt);
    void updateGame(float dt);
};

#endif // __HELLOWORLD_SCENE_H__
