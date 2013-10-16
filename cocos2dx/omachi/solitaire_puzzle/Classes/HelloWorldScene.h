#ifndef __HELLOWORLD_SCENE_H__
#define __HELLOWORLD_SCENE_H__

#include <vector>
#include "cocos2d.h"

class HelloWorld : public cocos2d::CCLayer
{
	class Block{
		int value_;
		bool enable_;
		cocos2d::CCLabelTTF* label_;
		int x_;
		int y_;
		int size_;
	public:
		void init(int size,int x,int y,cocos2d::CCLabelTTF*);
		void setState(int);
		void setStateRandom();
		int getState() const { return value_; }
		void setEnable(bool);
		bool getEnable() const { return enable_; }
		static cocos2d::CCPoint position(int x,int y,int size);
	};

	static const int kHorizontalNumber = 8;
	static const int kVerticalNumber = 8;
	std::vector< Block > blocks_;

	cocos2d::CCLabelTTF* label_;
	int current_number_;
	int last_i_;
	int last_j_;
    static const int kMAX_NUMBER = 6;

	int block_size_;
public:
    // Here's a difference. Method 'init' in cocos2d-x returns bool, instead of returning 'id' in cocos2d-iphone
    virtual bool init();  

    // there's no 'id' in cpp, so we recommend returning the class instance pointer
    static cocos2d::CCScene* scene();
    
    // a selector callback
    void menuCloseCallback(CCObject* pSender);

    bool ccTouchBegan(cocos2d::CCTouch *ptouch, cocos2d::CCEvent *pEvent);

    // implement the "static node()" method manually
    CREATE_FUNC(HelloWorld);
private:
    Block& block(int x,int y);
    const Block& block(int x,int y) const;
};

#endif // __HELLOWORLD_SCENE_H__
