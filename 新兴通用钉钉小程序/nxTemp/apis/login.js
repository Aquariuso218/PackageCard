import request from "@/nxTemp/request/ajax.js";
// 用户登录
export function postLogin(data) {
	return request({
		url: '/CheckLogin/Login',
		method: 'POST',
		data
	})
}
