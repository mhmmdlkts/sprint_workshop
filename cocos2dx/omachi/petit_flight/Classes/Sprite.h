#ifndef __HELLOWORLD_SPRITE_H__
#define __HELLOWORLD_SPRITE_H__

#include "cocos2d.h"

class Sprite {
public:
	cocos2d::CCSprite* mSprite;
	float mX,mY;
	float mVelocityX,mVelocityY;
	float mAccelX,mAccelY;
	float mSize;
	float mResist;
	float mRotation;
	float mRotationVelocity;
	float mCount;
	bool mDestroyed;
	int mInfo;

	Sprite();
	void update(float dt, float cameraX);
	bool isCollide( const Sprite& ) const;
};

#endif
