import request from "@/nxTemp/request/ajax.js";

//查询入库产品列表
export function getStockInList(params) {
	return request({
		url: '/v1/packinglable/getStockInList',
		method: 'GET',
		params: params
	});
}

//扫码查询箱子详情列表！！
export function stockInList(barCode) {
	return request({
		url: '/v1/packinglable/stockInList',
		method: 'GET',
		params: {
			barCode
		}
	});
}

//查询发货单产品列表
export function getStockOutList(params) {
	return request({
		url: '/v1/packinglable/stockOutList',
		method: 'GET',
		params: params
	});
}

// 查询仓库列表
export function wareHouseList(pager, text) {
	return request({
		url: '/v1/packinglable/wareHouseList',
		method: 'GET',
		params: {
			...pager, // 展开分页参数
			text: text || undefined // 可选参数，传空时设为 undefined
		}
	});
}

// 查询客户列表
export function customerList(pager, text) {
	return request({
		url: '/v1/packinglable/customerList',
		method: 'GET',
		params: {
			...pager, // 展开分页参数
			text: text || undefined // 可选参数，传空时设为 undefined
		}
	});
}

// 产品入库
export function stockIn(salesInvoiceQuery) {
	return request({
		url: '/v1/packinglable/stockIn',
		method: 'POST',
		data: salesInvoiceQuery // 请求体参数
	});
}

// 销售出库
export function salesInvoice(salesInvoiceQuery) {
	return request({
		url: '/v1/packinglable/salesInvoice',
		method: 'POST',
		data: salesInvoiceQuery // 请求体参数
	});
}

export function getUserInfo(data) {
	return request({
		url: '/v1/dingding/getUserInfo',
		method: 'POST',
		data
	})
}

/**订单确认相关接口 */
// 订单主表查询
export function orderMainList(data) {
	return request({
		url: '/POMainOrder/GetOrderMainList',
		method: 'POST',
		data
	})
}
// 订单明细查询
export function orderDetailList(data) {
	return request({
		url: '/POMainOrder/GetOrderdetailsList',
		method: 'POST',
		data
	})
}
//订单确认
export function orderConfirm(data) {
	return request({
		url: '/POMainOrder/SetOrderQueDing',
		method: 'POST',
		data
	})
}

/**订单查询相关接口 */
//订单列表
export function orderList(data) {
	return request({
		url: '/POMainOrder/GetOrderListSel',
		method: 'POST',
		data
	})
}

//供应商下拉树
export function venList(data) {
	return request({
		url: '/POMainOrder/GetcVenCode',
		method: 'POST',
		data
	})
}
//产品下拉树
export function invList(data) {
	return request({
		url: '/POMainOrder/GetcInvCode',
		method: 'POST',
		data
	})
}

//产品下拉树
export function personList(data) {
	return request({
		url: '/POMainOrder/GetPerson',
		method: 'POST',
		data
	})
}

/** 报表相关接口 */
//解密手机号
export function GetPhone(data) {
	return request({
		url: '/CheckLogin/LoginPhone',
		method: 'POST',
		data
	})
}

//获取微信的用户id
export function GetUserID(data) {
	return request({
		url: '/CheckLogin/getWxAPPID',
		method: 'POST',
		data
	})
}

export function SetPhone(data) {
	return request({
		url: '/CheckLogin/setWxOpenIDMobile',
		method: 'POST',
		data
	})
}

//工序表
export function GetOrderByHY(data) {
	return request({
		url: '/POMainOrder/GetOrderByHY',
		method: 'POST',
		data
	})
}
//锻坯表
export function GetOrderByPO(data) {
	return request({
		url: '/POMainOrder/GetOrderByPO',
		method: 'POST',
		data
	})
}