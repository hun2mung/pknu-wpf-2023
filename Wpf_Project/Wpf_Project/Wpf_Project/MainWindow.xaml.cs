using MahApps.Metro.Controls;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf_Project.Logics;
using Wpf_Project.Models;
using System.Text.RegularExpressions;
using ControlzEx.Standard;

namespace Wpf_Project
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // 처음부터 조회창 활성화
        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            BtnTourSearch_Click(sender, e);
        }

        // 조회 버튼
        private async void BtnTourSearch_Click(object sender, RoutedEventArgs e)
        {
            string openApiUri = "https://www.gimhae.go.kr/openapi/tour/tourinfo.do";
            string result = string.Empty;

            // WebRequest, WebResponse 객체 필요
            WebRequest req = null;
            WebResponse res = null;
            StreamReader reader = null;

            try
            {
                req = WebRequest.Create(openApiUri);
                res = await req.GetResponseAsync();
                reader = new StreamReader(res.GetResponseStream());
                result = reader.ReadToEnd();

            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"OpenAPI 조회오류 : {ex.Message}");
            }

            var jsonResult = JObject.Parse(result);

            var serRes = 0;

            try
            {
                var data = jsonResult["results"];
                var json_array = data as JArray;

                var tourinfo = new List<TourInfo>();

                foreach (var item in json_array)
                {
                    var imgArray = item["images"] as JArray;

                    tourinfo.Add(new TourInfo
                    {
                        Name = Convert.ToString(item["name"]),
                        Category = Convert.ToString(item["categroy"]),
                        Phone = Convert.ToString(item["phone"]),
                        Area = Convert.ToString(item["area"]),
                        Content = Convert.ToString(item["content"]),
                        Xposition = Convert.ToDouble(item["xposition"]),
                        Yposition = Convert.ToDouble(item["yposition"]),
                        Images = Convert.ToString(imgArray[0])
                    });

                    serRes += 1;
                }
                this.DataContext = tourinfo;
                StsResult.Content = $"{serRes}건 조회 완료!";
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"JSON 처리오류 : {ex.Message}");
            }
        }


        // DB 저장된 데이터 검색 버튼
        private async void BtnFind_Click(object sender, RoutedEventArgs e)
        {
            var findRes = 0;
            string tName = TxtSearch.Text;
            //MessageBox.Show(TxtSearch.Text.ToString());
            if (GrdResult.Items.Count == 0)
            {
                await Commons.ShowMessageAsync("오류", "검색할 데이터가 없습니다.");
                return;
            }

            List<TourInfo> list = new List<TourInfo>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(Commons.myConnString))
                {
                    if (conn.State == System.Data.ConnectionState.Closed) { conn.Open(); }
                    var query = $@"SELECT * FROM tourinfo WHERE Name LIKE '%{tName}%'";

                    var cmd = new MySqlCommand(query, conn);
                    var adapter = new MySqlDataAdapter(cmd);
                    var dSet = new DataSet();
                    adapter.Fill(dSet, "tourinfo");

                    foreach (DataRow dr in dSet.Tables["tourinfo"].Rows)
                    {
                        list.Add(new TourInfo
                        {
                            Id = Convert.ToInt32(dr["ID"]),
                            Name = Convert.ToString(dr["Name"]),
                            Category = Convert.ToString(dr["Category"]),
                            Phone = Convert.ToString(dr["Phone"]),
                            Area = Convert.ToString(dr["Area"]),
                            Content = Convert.ToString(dr["Content"]),
                            Xposition = Convert.ToDouble(dr["Xposition"]),
                            Yposition = Convert.ToDouble(dr["Yposition"]),
                            Images = Convert.ToString(dr["Images"])
                        });

                        findRes += cmd.ExecuteNonQuery();
                    }
                    this.DataContext = list;
                    StsResult.Content = $"{findRes}건 검색완료";

                }
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"검색 오류! : {ex.Message}");
            }

        }

        // 그리드 선택 항목 더블 클릭
        private async void GrdResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GrdResult.SelectedItem == null)
            {
                await Commons.ShowMessageAsync("오류", "확인하고 싶은 곳을 선택하시오.");
            }
            else
            {
                var selItem = GrdResult.SelectedItem as TourInfo;

                var mapWindow = new Map(selItem.Xposition, selItem.Yposition); // 부모창 위치값을 자식장으로 전달
                mapWindow.Owner = this; // MainWindow부모
                mapWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner; // 부모창 중간에 출력
                mapWindow.ShowDialog();
            }
        }

        // 검색 텍스트 창에서 엔터키 입력 시 검색 버튼 활성화
        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnFind_Click(sender, e);
            }
        }

        // 저장 버튼
        private async void BtnSaveData_Click(object sender, RoutedEventArgs e)
        {
            if (GrdResult.SelectedItems.Count == 0)
            {
                await Commons.ShowMessageAsync("오류", "저장할 데이터가 없습니다.");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(Commons.myConnString))
                {
                    if (conn.State == System.Data.ConnectionState.Closed) { conn.Open(); }

                    var query = @"INSERT INTO tourinfo
                                              (
                                              Name,
                                              Category,
                                              Phone,
                                              Area,
                                              Content,
                                              Xposition,
                                              Yposition,
                                              Images)
                                       VALUES
                                              (
                                              @Name,
                                              @Category,
                                              @Phone,
                                              @Area,
                                              @Content,
                                              @Xposition,
                                              @Yposition,
                                              @Images)";

                    var insRes = 0;
                    foreach (var temp in GrdResult.SelectedItems)
                    {
                        if (temp is TourInfo)
                        {
                            var info = temp as TourInfo;

                            MySqlCommand cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@Name", info.Name);
                            cmd.Parameters.AddWithValue("@Category", info.Category);
                            cmd.Parameters.AddWithValue("@Phone", info.Phone);
                            cmd.Parameters.AddWithValue("@Area", info.Area);
                            cmd.Parameters.AddWithValue("@Content", info.Content);
                            cmd.Parameters.AddWithValue("@Xposition", info.Xposition);
                            cmd.Parameters.AddWithValue("@Yposition", info.Yposition);
                            cmd.Parameters.AddWithValue("@images", info.Images);

                            insRes += await cmd.ExecuteNonQueryAsync();
                        }
                    }
                    await Commons.ShowMessageAsync("저장", $"{insRes}건 DB저장 성공!");
                    BtnFind_Click(sender, e);
                    StsResult.Content = $" DB 저장 {insRes}건 성공!";
                }
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"DB저장 오류! : {ex.Message}");
            }
        }

        // 삭제 버튼
        private async void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(Commons.myConnString))
                {
                    if (conn.State == ConnectionState.Closed) { conn.Open(); }

                    var query = @"DELETE FROM tourinfo WHERE Id=@Id";
                    var delRes = 0;

                    foreach (TourInfo item in GrdResult.SelectedItems)
                    {
                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@Id", item.Id);

                        delRes += cmd.ExecuteNonQuery();
                    }

                    if (delRes == GrdResult.SelectedItems.Count)
                    {
                        await Commons.ShowMessageAsync("삭제", $"{delRes}건 DB삭제성공!!");
                        StsResult.Content = $"즐겨찾기 {delRes} 건 삭제 완료";
                        BtnFind_Click(sender, e);
                    }
                    else
                    {
                        await Commons.ShowMessageAsync("삭제", $"DB삭제 {delRes} 일부 성공!!");   // 발생가능성 거의 없음.
                    }
                }
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"DB삭제 오류 {ex.Message}");
            }
        }

        private async void GrdResult_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                string tourPath = string.Empty;

                if (GrdResult.SelectedItem is TourInfo) // openAPI로 검색된 영화의 포스터ㅅ
                {
                    var place = GrdResult.SelectedItem as TourInfo;
                    tourPath = place.Images.ToString();

                }
                else if (GrdResult.SelectedItem is TourInfoDB)
                {
                    var place = GrdResult.SelectedItem as TourInfoDB;
                    tourPath = place.Images;
                }

                if (string.IsNullOrEmpty(tourPath)) // 포스터 이미지가 없으면 No_Picture
                {
                    ImgPicture.Source = new BitmapImage(new Uri("/No_Picture.png", UriKind.RelativeOrAbsolute));
                }
                else // 포스터이미지 경로가 있으면
                {                    
                    ImgPicture.Source = new BitmapImage(new Uri(tourPath, UriKind.RelativeOrAbsolute));
                }
            }
            catch
            {
                await Commons.ShowMessageAsync("오류", $"이미지로드 오류발생");
            }
        }





    }
}
