<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StarkDetail.ascx.cs" Inherits="RockWeb.Blocks.Utility.StarkDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-star"></i>
                    Blank Detail Block
                </h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlblTest" runat="server" LabelType="Info" Text="Label" />
                </div>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-md-6">
                        <Rock:PersonBasicEditor
                            ID="personEditor"
                            runat="server"
                            ShowInColumns="false"
                            PersonLabelPrefix=""
                            ShowTitle="true"
                            ShowSuffix="true"
                            ShowConnectionStatus="true"
                            ShowEmail="true"
                            ShowMobilePhone="true"
                            ShowGrade="false"
                            ShowMaritalStatus="false"
                            ShowPersonRole="false"
                            RequireGender="true"/>

                        <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="btnSave_Click" />
                    </div>
                    <div class="col-md-6">

                        
                    </div>
                </div>

            </div>
            
            

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
