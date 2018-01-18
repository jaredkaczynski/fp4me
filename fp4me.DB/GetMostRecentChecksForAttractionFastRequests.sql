DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `GetMostRecentChecksForAttractionFastRequests`(
	IN userID BIGINT(20), 
    IN checksToReturnPerAttractionFastRequest INT(11),
    IN attractionFastPassRequestStatus INT(11)
)
BEGIN
SELECT
	rankedChecks.*
FROM
	(SELECT
		IF(checks.AttractionFastPassRequestID=@lastAttractionFastPassRequestID,@currentRank:=@currentRank+1,@currentRank:=1) AS Rank,
		@lastAttractionFastPassRequestID:=checks.AttractionFastPassRequestID Sequence,
		checks.* 
	FROM
		(SELECT 
			r.AttractionFastPassRequestID,
            c.* 
		FROM 
			attractionfastpassrequests r
			join users u on r.UserID = u.UserID
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
			and r.Status = attractionFastPassRequestStatus
		ORDER BY
			r.AttractionFastPassRequestID, c.Timestamp desc) checks
		, (SELECT @currentRank:=0, @lastAttractionFastPassRequestID:=0) rank
		ORDER BY checks.AttractionFastPassRequestID, checks.Timestamp desc) rankedChecks
WHERE
	rankedChecks.rank <= checksToReturnPerAttractionFastRequest;
END$$
DELIMITER ;
