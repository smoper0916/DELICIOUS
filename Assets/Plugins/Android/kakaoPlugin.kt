package com.DefaultCompany.TastyConcern

import android.os.Bundle
import android.util.Log
import com.kakao.auth.ISessionCallback
import com.kakao.auth.Session
import com.kakao.util.exception.KakaoException
import com.unity3d.player.UnityPlayerActivity
import android.content.Intent
import androidx.annotation.Nullable
import com.kakao.auth.AuthType
import com.unity3d.player.UnityPlayer


class kakaoPlugin : UnityPlayerActivity(){

    var callback : SessionCallback?= null

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        callback = SessionCallback()
        Session.getCurrentSession().addCallback(callback)
    }

    fun Login(){
        Session.getCurrentSession().open(AuthType.KAKAO_LOGIN_ALL, UnityPlayer.currentActivity)
    }

    override fun onActivityResult(requestCode: Int, resultCode: Int, @Nullable data: Intent) {

        // 카카오톡|스토리 간편로그인 실행 결과를 받아서 SDK로 전달
        if (Session.getCurrentSession().handleActivityResult(requestCode, resultCode, data)) {
            return
        }

        super.onActivityResult(requestCode, resultCode, data)
    }

    override fun onDestroy() {
        super.onDestroy()

        // 세션 콜백 삭제
        Session.getCurrentSession().removeCallback(callback)
    }

    inner class SessionCallback : ISessionCallback{

        override fun onSessionOpened() {
            redirectSignupActivity()
        }

        override fun onSessionOpenFailed(exception: KakaoException?) {
            if(exception != null){
                Log.d("ppap",exception.toString())
            }
        }
    }

    protected fun redirectSignupActivity(){
        Log.d("ppap","로그인이 성공적으로 되었습니다.")
    }
}