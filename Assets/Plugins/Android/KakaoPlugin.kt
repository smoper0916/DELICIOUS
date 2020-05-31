package com.DefaultCompany.TastyConcern

import android.app.AlertDialog
import android.content.Intent
import android.os.Bundle
import android.util.Log
import com.kakao.auth.AuthType
import com.kakao.auth.ISessionCallback
import com.kakao.auth.Session
import com.kakao.network.ErrorResult
import com.kakao.usermgmt.UserManagement
import com.kakao.usermgmt.callback.LogoutResponseCallback
import com.kakao.usermgmt.callback.MeV2ResponseCallback
import com.kakao.usermgmt.callback.UnLinkResponseCallback
import com.kakao.usermgmt.response.MeV2Response
import com.kakao.util.exception.KakaoException
import com.unity3d.player.UnityPlayer
import com.unity3d.player.UnityPlayerActivity
import java.util.ArrayList

class KakaoPlugin : UnityPlayerActivity() {
    var callback: SessionCallback? = null

    override fun onCreate(p0: Bundle?) {
        super.onCreate(p0)

        callback = SessionCallback()
        Session.getCurrentSession().addCallback(callback)
    }

    fun Login() : String{
        Session.getCurrentSession().open(AuthType.KAKAO_LOGIN_ALL, UnityPlayer.currentActivity)
        val Keys = ArrayList<String>()
        UserManagement.getInstance().me(Keys,object: MeV2ResponseCallback(){
            override fun onSuccess(result: MeV2Response?){
                Log.d("ppap",result.toString())

            }

            override fun onSessionClosed(errorResult: ErrorResult?) {
                Log.d("ppap","세션이 끊겨있어서 실패")
            }

        })
        Log.d("ppap",Keys.toString())
        return Keys.toString()
    }

    fun Logout(){
        UserManagement.getInstance().requestLogout(object : LogoutResponseCallback() {
            override fun onCompleteLogout() {
                Log.i("ppap", "로그아웃 완료")
            }
        })
    }

    fun UnLink(){
        UnityPlayer.currentActivity.runOnUiThread{
            var dialog = AlertDialog.Builder(UnityPlayer.currentActivity)


            dialog.setTitle("영구 탈퇴 안내")
            dialog.setMessage("탈퇴시 모든 데이터가 삭제됩니다.")

            dialog.setNegativeButton("취소"){
                    dialog, which->
                dialog.dismiss()
            }

            dialog.setPositiveButton("예"){
                    dialgo, which->
                UserManagement.getInstance().requestUnlink(object: UnLinkResponseCallback(){
                    override fun onSuccess(result: Long?) {
                        Log.d("ppap","탈퇴 되었습니다.")
                    }

                    override fun onSessionClosed(errorResult: ErrorResult?) {
                        Log.d("ppap","세션이 닫혔습니다.")
                    }

                })


            }

            dialog.show()
        }
    }
    //사용자 정보를 유니티에서 확인할 수 있을지 테스트
    fun Getme(){
        val Keys = ArrayList<String>()
        UserManagement.getInstance().me(Keys,object: MeV2ResponseCallback(){
            override fun onSuccess(result: MeV2Response?) {
                Log.d("ppap",result.toString())
            }

            override fun onSessionClosed(errorResult: ErrorResult?) {
                Log.d("ppap","세션이 끊겨있어서 실패")
            }

        })
    }

    override fun onActivityResult(requestCode: Int, resultCode: Int, data: Intent) {

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

    inner class SessionCallback : ISessionCallback {

        //아마도 로그인 시도했을때 성공하면 호출되는 함수인것 같음
        override fun onSessionOpened() {
            redirectSignupActivity()
        }
        //로그인 실패 했을 경우 호출됨
        override fun onSessionOpenFailed(exception: KakaoException?) {
            if(exception != null){
                Log.d("ppap",exception.toString())
            }
        }
    }
    //로그인 성공시 작업을 처리해주는 부분일것으로 예상
    //로그인 성공시 유저 정보를 담아서 넘겨 보겠음
    protected  fun redirectSignupActivity(){
        Log.d("ppap","로그인 성공")
    }
}