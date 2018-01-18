DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `GetAttractionFastPassRequestsWithStatistics`(IN userID BIGINT(20))
BEGIN
SELECT 
    r.UserID,
    a.AttractionID,
	r.AttractionFastPassRequestID,
    r.Status,
	p.Name ParkName,
    a.Name AttractionName,
    r.Date,
    count(c.NumberOfPeople) NumberOfChecks,
    CAST(sum(case when c.AttractionFastPassStatus = "AVAILABLE" then 1 else 0 end) AS SIGNED) NumberOfAvailableChecks,
    max(case when c.AttractionFastPassStatus = "AVAILABLE" then c.Timestamp else null end) LastAvailableCheck,
    max(c.Timestamp) LastCheckDate,
	u.SMSNotificationPhoneNumber,
    r.NumberOfPeople,
    r.LastCheckStatus, 
    r.LastCheckTimestamp    
FROM attractionfastpassrequests r
join users u on r.UserID = u.UserID
join attractions a on r.AttractionID = a.AttractionID
join parks p on p.ParkID = a.ParkID
join userplans up on u.UserPlanID = up.UserPlanID
left join attractionfastpassrequestchecks c
  on r.Date = c.Date
  and r.AttractionID = c.AttractionID
  and c.Timestamp > r.CreatedOn
  and (
	(c.AttractionFastPassStatus = "AVAILABLE" and c.NumberOfPeople >= r.NumberOfPeople)
    or 
    (c.AttractionFastPassStatus != "AVAILABLE" and c.NumberOfPeople <= r.NumberOfPeople))
WHERE
	u.userid = userID
group by
	u.SMSNotificationPhoneNumber,
    a.Name,
    p.Name,
    r.AttractionFastPassRequestID,
    r.UserID,
    a.AttractionID,
    r.Date,
    r.NumberOfPeople,
    r.LastCheckStatus, 
    r.LastCheckTimestamp, 
    r.Status;
END$$
DELIMITER ;
