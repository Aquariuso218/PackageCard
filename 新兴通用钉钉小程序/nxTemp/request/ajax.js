import ajax from '@/uni_modules/u-ajax/js_sdk'
import { BASE_URL } from '@/env.js'

// 创建请求实例
const instance = ajax.create({
    baseURL: BASE_URL
})

// 错误处理函数
const handleError = (error) => {
    // 提取错误信息并显示提示框
    let message = error?.errMsg || '后端或网络异常，请稍后再试';
    if (message.includes('request:fail')) {
        message = '后端访问异常！'  
    }else{
        message = '接口访问异常，请联系管理员！'
    }
    uni.showToast({ title: message, icon: 'none' });
    return Promise.reject(error);
}

// 添加请求拦截器
instance.interceptors.request.use(
    config => {
        // 可以在这里处理请求前的配置，比如添加 token 等
        return config;
    },
    error => {
        // console.log('请求错误', JSON.stringify(error));
        handleError(error);  // 调用错误处理函数
        return Promise.reject(error);
    }
)

// 添加响应拦截器
instance.interceptors.response.use(
    response => {
        if (response.statusCode !== 200) {
            // 非 200 状态码时弹出请求异常的提示框
            uni.showToast({ title: '请求异常！', icon: 'none' });
            return Promise.reject('network error');
        }
        // 可以根据业务需求在这里做进一步的数据校验
        // if (!response.data || response.data.success !== true) {
        //     uni.showToast({ title: '接口返回数据异常', icon: 'none' });
        //     return Promise.reject('data error');
        // }
        return response.data;
    },
    handleError  // 统一的错误处理
)

/**
 * 统一的请求方法，处理GET请求
 * @param {String} url [请求的url地址]
 * @param {Object} params [请求时携带的参数]
 */
export function get(url, params) {
    return instance.get(url, { params })
        .then(res => res.data)
        .catch(handleError);
}

/**
 * 统一的POST请求方法
 * @param {String} url [请求的url地址]
 * @param {Object} params [请求时携带的参数]
 */
export function post(url, params) {
    return instance.post(url, params)
        .then(res => res.data)
        .catch(handleError);
}

/**
 * 提交表单
 * @param {String} url [请求的url地址]
 * @param {Object} data [提交的数据]
 * @param {Object} config [配置参数]
 */
export function postForm(url, data, config) {
    return instance.post(url, data, config)
        .then(res => res.data)
        .catch(handleError);
}

export const install = Vue => {
    // 如果您是像我下面这样挂载在 Vue 原型链上，则通过 this.$ajax 调用
    Vue.prototype.$ajax = instance;
}

export default instance;
