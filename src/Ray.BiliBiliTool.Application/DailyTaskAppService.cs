﻿using System;
using Microsoft.Extensions.Logging;
using Ray.BiliBiliTool.Agent.Dtos;
using Ray.BiliBiliTool.Application.Contracts;
using Ray.BiliBiliTool.DomainService.Interfaces;

namespace Ray.BiliBiliTool.Application
{
    public class DailyTaskAppService : IDailyTaskAppService
    {
        private readonly ILogger<DailyTaskAppService> _logger;
        private readonly IAccountDomainService _loginDomainService;
        private readonly IVideoDomainService _videoDomainService;
        private readonly IMangaDomainService _mangaDomainService;
        private readonly ILiveDomainService _liveDomainService;
        private readonly IVipPrivilegeDomainService _vipPrivilegeDomainService;
        private readonly IChargeDomainService _chargeDomainService;

        public DailyTaskAppService(
            ILogger<DailyTaskAppService> logger,
            IAccountDomainService loginDomainService,
            IVideoDomainService videoDomainService,
            IMangaDomainService mangaDomainService,
            ILiveDomainService liveDomainService,
            IVipPrivilegeDomainService vipPrivilegeDomainService,
            IChargeDomainService chargeDomainService)
        {
            _logger = logger;
            _loginDomainService = loginDomainService;
            _videoDomainService = videoDomainService;
            _mangaDomainService = mangaDomainService;
            _liveDomainService = liveDomainService;
            _vipPrivilegeDomainService = vipPrivilegeDomainService;
            _chargeDomainService = chargeDomainService;
        }

        /// <summary>
        /// 记录当前登录账户信息
        /// </summary>
        private UseInfo _userInfo;

        /// <summary>
        /// 记录当前完成任务状态
        /// </summary>
        private DailyTaskInfo _dailyTaskInfo;


        public void DoDailyTask()
        {
            //登录
            _userInfo = _loginDomainService.LoginByCookie();
            if (_userInfo == null) throw new Exception("登录失败，请检查Cookie");

            //获取任务完成情况
            _dailyTaskInfo = _loginDomainService.GetDailyTaskStatus();

            //观看、分享视频
            _videoDomainService.WatchAndShareVideo(_dailyTaskInfo);
            //投币任务
            _videoDomainService.AddCoinsForVideo();

            //直播中心签到
            _liveDomainService.LiveSign();
            //直播中心的银瓜子兑换硬币
            _userInfo.Money = _liveDomainService.ExchangeSilver2Coin();

            //月初领取大会员福利
            _vipPrivilegeDomainService.ReceiveVipPrivilege(_userInfo);

            //月底充电
            _chargeDomainService.Charge(_userInfo);

            //漫画签到
            _mangaDomainService.MangaSign();
            //获取每月大会员漫画权益
            _mangaDomainService.ReceiveMangaVipReward(1, _userInfo);
        }
    }
}
