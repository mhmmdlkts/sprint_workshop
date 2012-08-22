package jp.co.happyelements.Todo;

import android.app.Activity;
import android.os.Bundle;
import android.widget.ImageView;

public class Danbo extends Activity
{
    /** Called when the activity is first created. */
    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        ImageView imageView = new ImageView(this);
        imageView.setImageResource(R.drawable.danbo);
    }
}
