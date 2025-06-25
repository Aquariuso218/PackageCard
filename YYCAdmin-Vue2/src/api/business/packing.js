import request from '@/utils/request'

// 删除箱子
export function delBox(boxNumber) {
  return request({
    url: 'v1/packinglable/DeletePC/' + boxNumber,
    method: 'delete',
  })
}

// 删除箱子
export function delBoxs(id) {
  return request({
    url: 'v1/packinglable/DeletePCD/' + id,
    method: 'delete',
  })
}



//特殊装箱
export function update(data){
  return request({
    url:'v1/packinglable/update',
    method:'post',
    data: data,
  })
}


//产品列表
export function invList(query) {
  return request({
    url: 'v1/packinglable/getInvList',
    method: 'get',
    params: query,
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

//装箱
export function createPacking(data){
  return request({
    url:'v1/packinglable/create',
    method:'post',
    data: data,
  })
}

//合并装箱
export function mergeCreatePacking(data){
  return request({
    url:'v1/packinglable/mergeCreate',
    method:'post',
    data: data,
  })
}

//特殊装箱
export function specialCreatePacking(data){
  return request({
    url:'v1/packinglable/createSpcPackage',
    method:'post',
    data: data,
  })
}



