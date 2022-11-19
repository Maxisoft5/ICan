select 
 p.ProductID , 
  p.Name, 
  getOrderAmount(p.ProductId, 2021,1)  as  'январь',
  getOrderAmount(p.ProductId, 2021,2)  as  'февраль',
  getOrderAmount(p.ProductId, 2021,3)  as  'март',
  getOrderAmount(p.ProductId, 2021,4)  as  'апрель',
  getOrderAmount(p.ProductId, 2021,5)  as  'май',
  getOrderAmount(p.ProductId, 2021,6)  as  'июнь',
  getOrderAmount(p.ProductId, 2021,7)  as  'июль',
  getOrderAmount(p.ProductId, 2021,8)  as  'август',
  getOrderAmount(p.ProductId, 2021,9)  as  'сентябрь',
  getOrderAmount(p.ProductId, 2021,10)  as  'октябрь',
  getOrderAmount(p.ProductId, 2021,11)  as  'ноябрь'  
  from opt_product p 
join opt_productseries s on s.ProductSeriesID = p.ProductSeriesID
where p.CountryId = 2 and 
p.ProductKindID = 1  and 
p.Name not like '%Подарочные%'
 order by  s.Order,  p.DisplayOrder