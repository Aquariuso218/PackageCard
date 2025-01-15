import request from '@/utils/request'

//产品列表
export function invList(query) {
  return request({
    url: 'v1/packinglable/getInvList',
    method: 'get',
    params: query,
  })
}

//装箱
export function createPacking(data){
  return request({
    url:'v1/packinglable/create',
    method:'post',
    data: data,
  })
}

// 装箱列表
export function packingList(query){
  return request({
    url: 'v1/packinglable/getPCList',
    method: 'get',
    params: query,
  })
}