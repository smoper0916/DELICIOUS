package com.delicious.ohtasty

import android.app.Application
import com.kakao.auth.*

class GlobalApplication : Application() {
    class KakaoSdkAdapter : KakaoAdapter(){

        override fun getSessionConfig(): ISessionConfig {
            return object : ISessionConfig {
                override fun getAuthTypes(): Array<AuthType> {
                    return arrayOf(AuthType.KAKAO_LOGIN_ALL)
                }

                override fun isUsingWebviewTimer(): Boolean {
                    return false
                }

                override fun isSecureMode(): Boolean {
                    return false
                }

                override fun getApprovalType(): ApprovalType? {
                    return ApprovalType.INDIVIDUAL
                }

                override fun isSaveFormData(): Boolean {
                    return true
                }
            }
        }

        override fun getApplicationConfig(): IApplicationConfig {
            return IApplicationConfig { globalApplicationContext }
        }
    }

    override fun onCreate() {
        super.onCreate()
        instance=this
        KakaoSDK.init(KakaoSdkAdapter())
    }

    companion object{
        private var instance : GlobalApplication? = null
        val globalApplicationContext:GlobalApplication
            get(){
                if(instance == null)
                    throw IllegalStateException("wow")

                return instance!!
            }
    }
}