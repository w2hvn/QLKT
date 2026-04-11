with open('qlkt/Views/ReportManagement.xaml.cs', 'r') as f:
    content = f.read()

content = content.replace('FROM Rewards r', 'FROM Proposals r')
content = content.replace('r.RewardType', 'c.CategoryName')
content = content.replace('LEFT JOIN Units u', 'LEFT JOIN Units u ON s.UnitID = u.UnitID LEFT JOIN RewardCategories c ON r.CategoryID = c.CategoryID')
content = content.replace('ON s.UnitID = u.UnitID LEFT JOIN RewardCategories c ON r.CategoryID = c.CategoryID\n                                      LEFT JOIN Units u ON s.UnitID = u.UnitID', 'LEFT JOIN RewardCategories c ON r.CategoryID = c.CategoryID')
content = content.replace('r.DateSigned', 'r.DateProposed')
content = content.replace('AND r.RewardType', 'AND c.CategoryName')

with open('qlkt/Views/ReportManagement.xaml.cs', 'w') as f:
    f.write(content)
